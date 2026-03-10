#!/bin/bash

# Phase 2: Update Namespace Declarations
echo "========================================"
echo "Phase 2: Update Namespace Declarations"
echo "========================================"
echo ""

# Find all C# files
echo "Finding C# files..."
cs_files=$(find . -name "*.cs" -type f ! -path "*/bin/*" ! -path "*/obj/*" ! -path "*/.git/*")
total_files=$(echo "$cs_files" | wc -l)
echo "Found $total_files C# files"
echo ""

modified_count=0

# Process each file
while IFS= read -r file; do
    if [ -f "$file" ]; then
        # Create a temporary file
        temp_file="${file}.tmp"

        # Apply all namespace replacements
        sed -E \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)cqrs\.behaviors/\1 \2CQRS.Behaviors/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)cqrs\.command/\1 \2CQRS.Command/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)cqrs\.notification/\1 \2CQRS.Notification/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)cqrs\.query/\1 \2CQRS.Query/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)cqrs\.request/\1 \2CQRS.Request/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)coroutine\.extensions/\1 \2Coroutine.Extensions/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)coroutine\.instructions/\1 \2Coroutine.Instructions/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)functional\.async/\1 \2Functional.Async/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)functional\.control/\1 \2Functional.Control/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)functional\.functions/\1 \2Functional.Functions/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)functional\.pipe/\1 \2Functional.Pipe/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)functional\.result/\1 \2Functional.Result/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)logging\.appenders/\1 \2Logging.Appenders/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)logging\.filters/\1 \2Logging.Filters/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)logging\.formatters/\1 \2Logging.Formatters/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)services\.modules/\1 \2Services.Modules/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)setting\.events/\1 \2Setting.Events/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)setting\.data/\1 \2Setting.Data/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)scene\.handler/\1 \2Scene.Handler/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)ui\.handler/\1 \2UI.Handler/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)extensions\.signal/\1 \2Extensions.Signal/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)events\.filters/\1 \2Events.Filters/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)data\.events/\1 \2Data.Events/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)cqrs([^a-z])/\1 \2CQRS\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)ioc([^a-z])/\1 \2IoC\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)ui([^a-z])/\1 \2UI\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)ecs([^a-z])/\1 \2ECS\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)architecture([^a-z])/\1 \2Architecture\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)bases([^a-z])/\1 \2Bases\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)command([^a-z])/\1 \2Command\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)configuration([^a-z])/\1 \2Configuration\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)controller([^a-z])/\1 \2Controller\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)coroutine([^a-z])/\1 \2Coroutine\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)data([^a-z])/\1 \2Data\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)enums([^a-z])/\1 \2Enums\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)environment([^a-z])/\1 \2Environment\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)events([^a-z])/\1 \2Events\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)extensions([^a-z])/\1 \2Extensions\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)functional([^a-z])/\1 \2Functional\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)internals([^a-z])/\1 \2Internals\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)lifecycle([^a-z])/\1 \2Lifecycle\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)logging([^a-z])/\1 \2Logging\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)model([^a-z])/\1 \2Model\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)pause([^a-z])/\1 \2Pause\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)pool([^a-z])/\1 \2Pool\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)properties([^a-z])/\1 \2Properties\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)property([^a-z])/\1 \2Property\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)query([^a-z])/\1 \2Query\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)registries([^a-z])/\1 \2Registries\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)resource([^a-z])/\1 \2Resource\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)rule([^a-z])/\1 \2Rule\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)serializer([^a-z])/\1 \2Serializer\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)services([^a-z])/\1 \2Services\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)state([^a-z])/\1 \2State\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)storage([^a-z])/\1 \2Storage\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)system([^a-z])/\1 \2System\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)time([^a-z])/\1 \2Time\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)utility([^a-z])/\1 \2Utility\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)versioning([^a-z])/\1 \2Versioning\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)scene([^a-z])/\1 \2Scene\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)setting([^a-z])/\1 \2Setting\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)diagnostics([^a-z])/\1 \2Diagnostics\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)components([^a-z])/\1 \2Components\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)systems([^a-z])/\1 \2Systems\3/g' \
            -e 's/(namespace|using|using static)\s+(GFramework[^;{]*\.)analyzers([^a-z])/\1 \2Analyzers\3/g' \
            "$file" > "$temp_file"

        # Check if file was modified
        if ! cmp -s "$file" "$temp_file"; then
            mv "$temp_file" "$file"
            echo "✓ Updated: ${file#./}"
            ((modified_count++))
        else
            rm "$temp_file"
        fi
    fi
done <<< "$cs_files"

echo ""
echo "========================================"
echo "Summary:"
echo "  Total files scanned: $total_files"
echo "  Files modified: $modified_count"
echo "========================================"
echo ""

if [ $modified_count -gt 0 ]; then
    echo "Committing namespace updates..."
    git add -A
    git commit -m "refactor: update namespace declarations to PascalCase (phase 2)"
    echo "✓ Phase 2 complete"
else
    echo "No files needed updating"
fi
