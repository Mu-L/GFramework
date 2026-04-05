using System.IO;
using GFramework.Game.Abstractions.Data;
using GFramework.Game.Data;
using GFramework.Game.Serializer;
using GFramework.Game.Storage;

namespace GFramework.Game.Tests.Data;

[TestFixture]
public class PersistenceTests
{
    private static string CreateTempRoot()
    {
        var path = Path.Combine(Path.GetTempPath(), "gframework-persistence", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    [Test]
    public async Task FileStorage_PersistsDataAndRejectsIllegalKeys()
    {
        var root = CreateTempRoot();
        using var storage = new FileStorage(root, new JsonSerializer(), ".json");

        var saved = new TestSimpleData { Value = 5 };
        await storage.WriteAsync("folder/item", saved);

        var loaded = await storage.ReadAsync<TestSimpleData>("folder/item");
        Assert.That(loaded.Value, Is.EqualTo(saved.Value));

        Assert.ThrowsAsync<ArgumentException>(async () => await storage.WriteAsync("../escape", new TestSimpleData()));
    }

    [Test]
    public async Task SaveRepository_ManagesSlots()
    {
        var root = CreateTempRoot();
        using var storage = new FileStorage(root, new JsonSerializer());
        var config = new SaveConfiguration
        {
            SaveRoot = "saves",
            SaveSlotPrefix = "slot_",
            SaveFileName = "save.json"
        };

        var repository = new SaveRepository<TestSaveData>(storage, config);
        var data = new TestSaveData { Name = "hero" };

        await repository.SaveAsync(1, data);
        Assert.That(await repository.ExistsAsync(1));

        var loaded = await repository.LoadAsync(1);
        Assert.That(loaded.Name, Is.EqualTo(data.Name));

        var slots = await repository.ListSlotsAsync();
        Assert.That(slots, Is.EqualTo(new[] { 1 }));

        await repository.DeleteAsync(1);
        Assert.That(await repository.ExistsAsync(1), Is.False);
    }

    [Test]
    public async Task UnifiedSettingsDataRepository_RoundTripsDataAndLoadAll()
    {
        var root = CreateTempRoot();
        using var storage = new FileStorage(root, new JsonSerializer());
        var serializer = new JsonSerializer();
        var repo = new UnifiedSettingsDataRepository(
            storage,
            serializer,
            new DataRepositoryOptions { EnableEvents = false },
            "settings.json");

        var location = new TestDataLocation("settings/choice");
        repo.RegisterDataType(location, typeof(TestSimpleData));

        var data = new TestSimpleData { Value = 42 };
        await repo.SaveAsync(location, data);

        using var storage2 = new FileStorage(root, new JsonSerializer());
        var repo2 = new UnifiedSettingsDataRepository(
            storage2,
            serializer,
            new DataRepositoryOptions { EnableEvents = false },
            "settings.json");
        repo2.RegisterDataType(location, typeof(TestSimpleData));

        var loaded = await repo2.LoadAsync<TestSimpleData>(location);
        Assert.That(loaded.Value, Is.EqualTo(data.Value));

        var all = await repo2.LoadAllAsync();
        Assert.That(all.Keys, Contains.Item(location.Key));
        Assert.That(all[location.Key], Is.TypeOf<TestSimpleData>());
    }
}