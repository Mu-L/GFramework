/**
 * Parse a minimal JSON schema document used by the config extension.
 * The parser intentionally supports the same schema subset that the current
 * runtime validator and source generator depend on.
 *
 * @param {string} content Raw schema JSON text.
 * @returns {{required: string[], properties: Record<string, {type: string, itemType?: string}>}} Parsed schema info.
 */
function parseSchemaContent(content) {
    const parsed = JSON.parse(content);
    const required = Array.isArray(parsed.required)
        ? parsed.required.filter((value) => typeof value === "string")
        : [];
    const properties = {};
    const propertyBag = parsed.properties || {};

    for (const [key, value] of Object.entries(propertyBag)) {
        if (!value || typeof value !== "object" || typeof value.type !== "string") {
            continue;
        }

        if (value.type === "array" &&
            value.items &&
            typeof value.items === "object" &&
            typeof value.items.type === "string") {
            properties[key] = {
                type: "array",
                itemType: value.items.type
            };
            continue;
        }

        properties[key] = {
            type: value.type
        };
    }

    return {
        required,
        properties
    };
}

/**
 * Parse a minimal top-level YAML structure for config validation and form
 * preview. This parser intentionally focuses on the repository's current
 * config conventions: one root mapping object per file, top-level scalar
 * fields, and top-level scalar arrays.
 *
 * @param {string} text YAML text.
 * @returns {{entries: Map<string, {kind: string, value?: string, items?: Array<{raw: string, isComplex: boolean}>}>, keys: Set<string>}} Parsed YAML.
 */
function parseTopLevelYaml(text) {
    const entries = new Map();
    const keys = new Set();
    const lines = text.split(/\r?\n/u);

    for (let index = 0; index < lines.length; index += 1) {
        const line = lines[index];
        if (!line || line.trim().length === 0 || line.trim().startsWith("#")) {
            continue;
        }

        if (/^\s/u.test(line)) {
            continue;
        }

        const match = /^([A-Za-z0-9_]+):(?:\s*(.*))?$/u.exec(line);
        if (!match) {
            continue;
        }

        const key = match[1];
        const rawValue = match[2] || "";
        keys.add(key);

        if (rawValue.length > 0 && !rawValue.startsWith("|") && !rawValue.startsWith(">")) {
            entries.set(key, {
                kind: "scalar",
                value: rawValue.trim()
            });
            continue;
        }

        const childLines = [];
        let cursor = index + 1;
        while (cursor < lines.length) {
            const childLine = lines[cursor];
            if (childLine.trim().length === 0 || childLine.trim().startsWith("#")) {
                cursor += 1;
                continue;
            }

            if (!/^\s/u.test(childLine)) {
                break;
            }

            childLines.push(childLine);
            cursor += 1;
        }

        if (childLines.length === 0) {
            entries.set(key, {
                kind: "empty"
            });
            continue;
        }

        const arrayItems = parseTopLevelArray(childLines);
        if (arrayItems) {
            entries.set(key, {
                kind: "array",
                items: arrayItems
            });
            index = cursor - 1;
            continue;
        }

        entries.set(key, {
            kind: "object"
        });
        index = cursor - 1;
    }

    return {
        entries,
        keys
    };
}

/**
 * Produce extension-facing validation diagnostics from schema and parsed YAML.
 *
 * @param {{required: string[], properties: Record<string, {type: string, itemType?: string}>}} schemaInfo Parsed schema info.
 * @param {{entries: Map<string, {kind: string, value?: string, items?: Array<{raw: string, isComplex: boolean}>}>, keys: Set<string>}} parsedYaml Parsed YAML.
 * @returns {Array<{severity: "error" | "warning", message: string}>} Validation diagnostics.
 */
function validateParsedConfig(schemaInfo, parsedYaml) {
    const diagnostics = [];

    for (const requiredProperty of schemaInfo.required) {
        if (!parsedYaml.keys.has(requiredProperty)) {
            diagnostics.push({
                severity: "error",
                message: `Required property '${requiredProperty}' is missing.`
            });
        }
    }

    for (const key of parsedYaml.keys) {
        if (!Object.prototype.hasOwnProperty.call(schemaInfo.properties, key)) {
            diagnostics.push({
                severity: "error",
                message: `Property '${key}' is not declared in the matching schema.`
            });
        }
    }

    for (const [propertyName, propertySchema] of Object.entries(schemaInfo.properties)) {
        if (!parsedYaml.entries.has(propertyName)) {
            continue;
        }

        const entry = parsedYaml.entries.get(propertyName);
        if (propertySchema.type === "array") {
            if (entry.kind !== "array") {
                diagnostics.push({
                    severity: "error",
                    message: `Property '${propertyName}' is expected to be an array.`
                });
                continue;
            }

            for (const item of entry.items || []) {
                if (item.isComplex || !isScalarCompatible(propertySchema.itemType || "", item.raw)) {
                    diagnostics.push({
                        severity: "error",
                        message: `Array item in property '${propertyName}' is expected to be '${propertySchema.itemType}', but the current value is incompatible.`
                    });
                    break;
                }
            }

            continue;
        }

        if (entry.kind !== "scalar") {
            diagnostics.push({
                severity: "error",
                message: `Property '${propertyName}' is expected to be '${propertySchema.type}', but the current YAML shape is '${entry.kind}'.`
            });
            continue;
        }

        if (!isScalarCompatible(propertySchema.type, entry.value || "")) {
            diagnostics.push({
                severity: "error",
                message: `Property '${propertyName}' is expected to be '${propertySchema.type}', but the current scalar value is incompatible.`
            });
        }
    }

    return diagnostics;
}

/**
 * Determine whether a scalar value matches a minimal schema type.
 *
 * @param {string} expectedType Schema type.
 * @param {string} scalarValue YAML scalar value.
 * @returns {boolean} True when compatible.
 */
function isScalarCompatible(expectedType, scalarValue) {
    const value = unquoteScalar(scalarValue);
    switch (expectedType) {
        case "integer":
            return /^-?\d+$/u.test(value);
        case "number":
            return /^-?\d+(?:\.\d+)?$/u.test(value);
        case "boolean":
            return /^(true|false)$/iu.test(value);
        case "string":
            return true;
        default:
            return true;
    }
}

/**
 * Apply scalar field updates back into the original YAML text.
 *
 * @param {string} originalYaml Original YAML content.
 * @param {Record<string, string>} updates Updated scalar values.
 * @returns {string} Updated YAML content.
 */
function applyScalarUpdates(originalYaml, updates) {
    const lines = originalYaml.split(/\r?\n/u);
    const touched = new Set();

    const updatedLines = lines.map((line) => {
        if (/^\s/u.test(line)) {
            return line;
        }

        const match = /^([A-Za-z0-9_]+):(?:\s*(.*))?$/u.exec(line);
        if (!match) {
            return line;
        }

        const key = match[1];
        if (!Object.prototype.hasOwnProperty.call(updates, key)) {
            return line;
        }

        touched.add(key);
        return `${key}: ${formatYamlScalar(updates[key])}`;
    });

    for (const [key, value] of Object.entries(updates)) {
        if (touched.has(key)) {
            continue;
        }

        updatedLines.push(`${key}: ${formatYamlScalar(value)}`);
    }

    return updatedLines.join("\n");
}

/**
 * Format a scalar value for YAML output.
 *
 * @param {string} value Scalar value.
 * @returns {string} YAML-ready scalar.
 */
function formatYamlScalar(value) {
    if (/^-?\d+(?:\.\d+)?$/u.test(value) || /^(true|false)$/iu.test(value)) {
        return value;
    }

    if (value.length === 0 || /[:#\[\]\{\},]|^\s|\s$/u.test(value)) {
        return JSON.stringify(value);
    }

    return value;
}

/**
 * Remove a simple YAML string quote wrapper.
 *
 * @param {string} value Scalar value.
 * @returns {string} Unquoted value.
 */
function unquoteScalar(value) {
    if ((value.startsWith("\"") && value.endsWith("\"")) ||
        (value.startsWith("'") && value.endsWith("'"))) {
        return value.slice(1, -1);
    }

    return value;
}

/**
 * Parse a sequence of child lines as a top-level scalar array.
 *
 * @param {string[]} childLines Indented child lines.
 * @returns {Array<{raw: string, isComplex: boolean}> | null} Parsed array items or null when the block is not an array.
 */
function parseTopLevelArray(childLines) {
    const items = [];

    for (const line of childLines) {
        if (line.trim().length === 0 || line.trim().startsWith("#")) {
            continue;
        }

        const trimmed = line.trimStart();
        if (!trimmed.startsWith("-")) {
            return null;
        }

        const raw = trimmed.slice(1).trim();
        items.push({
            raw,
            isComplex: raw.length === 0 || raw.startsWith("{") || raw.startsWith("[") || /^[A-Za-z0-9_]+:\s*/u.test(raw)
        });
    }

    return items;
}

module.exports = {
    applyScalarUpdates,
    formatYamlScalar,
    isScalarCompatible,
    parseSchemaContent,
    parseTopLevelYaml,
    unquoteScalar,
    validateParsedConfig
};
