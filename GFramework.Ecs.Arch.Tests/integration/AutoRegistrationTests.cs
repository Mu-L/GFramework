using System.Linq;
using Arch.Core;
using GFramework.Core.Abstractions.properties;
using GFramework.Core.architecture;
using GFramework.Core.ioc;
using GFramework.Ecs.Arch.Abstractions;

namespace GFramework.Ecs.Arch.Tests.integration;

/// <summary>
/// 自动注册集成测试
/// </summary>
[TestFixture]
public class AutoRegistrationTests
{
    [SetUp]
    public void Setup()
    {
        _container = new MicrosoftDiContainer();
        _context = new ArchitectureContext(_container);
    }

    [TearDown]
    public void TearDown()
    {
        _container?.Clear();
        _context = null;
    }

    private MicrosoftDiContainer? _container;
    private ArchitectureContext? _context;

    /// <summary>
    /// 测试 Arch ECS 模块是否自动注册
    /// </summary>
    [Test]
    public void ArchEcsModule_Should_Be_Auto_Registered()
    {
        // Arrange - 手动触发模块初始化器（模拟自动注册）
        ArchModuleInitializer.Initialize();

        var services = new ArchitectureServices();
        var properties = new ArchitectureProperties();

        // Act
        services.ModuleManager.RegisterBuiltInModules(services.Container, properties);
        var modules = services.ModuleManager.GetModules();

        // Assert
        var archModule = modules.FirstOrDefault(m => m.ModuleName == nameof(ArchEcsModule));
        Assert.That(archModule, Is.Not.Null, "ArchEcsModule should be auto-registered");
        Assert.That(archModule, Is.InstanceOf<IArchEcsModule>());
    }

    /// <summary>
    /// 测试 World 是否正确注册到容器
    /// </summary>
    [Test]
    public void World_Should_Be_Registered_In_Container()
    {
        // Arrange - 手动触发模块初始化器
        ArchModuleInitializer.Initialize();

        var services = new ArchitectureServices();
        var properties = new ArchitectureProperties();

        // Act
        services.ModuleManager.RegisterBuiltInModules(services.Container, properties);
        services.ModuleManager.InitializeAllAsync(false).Wait();

        // Assert
        var world = services.Container.Get<World>();
        Assert.That(world, Is.Not.Null, "World should be registered in container");
    }
}