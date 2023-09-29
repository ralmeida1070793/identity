using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace IdentityTests.ServiceTests;

public class RoleServiceTests
{
    private Mock<RoleManager<IdentityRole>> roleManagerMock;
    
    [SetUp]
    public void Setup()
    {
        roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            new Mock<IRoleStore<IdentityRole>>().Object,
            new IRoleValidator<IdentityRole>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<IdentityRole>>>().Object
        );
    }
    
    [Test]
    public void CreateRole_Failed()
    {
        roleManagerMock.Setup(e => e.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        var service = new RoleServices(
            roleManagerMock.Object
        );

        try{
            Task.WaitAll(
                service
                    .CreateRole(
                        new RoleModel()
                        {
                            Name = "Test"
                        }, 
                        CancellationToken.None
                    )
            );
        
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.AreEqual(e.Message, "One or more errors occurred. (Role already exists)");
            roleManagerMock.Verify(e => e.CreateAsync(It.IsAny<IdentityRole>()), Times.Exactly(0));
        }
    }
    
    [Test]
    public void CreateRole_Success()
    {
        roleManagerMock.Setup(e => e.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        roleManagerMock.Setup(e => e.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
        var service = new RoleServices(
            roleManagerMock.Object
        );

        Task.WaitAll(
            service
                .CreateRole(
                    new RoleModel()
                    {
                        Name = "Test"
                    }, 
                    CancellationToken.None
                )
        );
        
        roleManagerMock.Verify(e => e.CreateAsync(It.IsAny<IdentityRole>()), Times.Exactly(1));
        
    }
    
    [Test]
    public void GetRole_Failed()
    {
        roleManagerMock.Setup(e => e.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(default(IdentityRole));
        var service = new RoleServices(
            roleManagerMock.Object
        );

        try{
            Task.WaitAll(
                service
                    .GetRole(
                        "Test", 
                        CancellationToken.None
                    )
            );
        
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.AreEqual(e.Message, "One or more errors occurred. (Role not found)");
        }
    }
    
    [Test]
    public void GetRole_Success()
    {
        roleManagerMock.Setup(e => e.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityRole() { Name = "Test"});
        var service = new RoleServices(
            roleManagerMock.Object
        );

        var result = service
            .GetRole(
                "Test", 
                CancellationToken.None
            ).Result;
        
        Assert.AreEqual(result.Name, "Test");
    }
}