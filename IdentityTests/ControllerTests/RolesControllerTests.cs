using System.Threading;
using Identity.Controllers;
using Identity.Models;
using Identity.Services;
using Moq;
using NUnit.Framework;

namespace IdentityTests.ControllerTests;

public class RolesControllerTests
{
    private Mock<IRoleServices> serviceMock = new Mock<IRoleServices>();
    
    [SetUp]
    public void Setup()
    {
        serviceMock.Setup(e => e.CreateRole(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()));
    }
    
    [Test]
    public void CreateRoleServiceAvailability_Success()
    {
        var controller = new RolesController(serviceMock.Object);
        controller.CreateRole(new RoleModel()
        {
            Name = "Test"
        }, CancellationToken.None);
        
        serviceMock.Verify(r => r.CreateRole(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
    }
    
}