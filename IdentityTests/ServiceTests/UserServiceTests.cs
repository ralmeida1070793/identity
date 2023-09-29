using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace IdentityTests.ServiceTests;

public class UserServiceTests
{
    private Mock<UserManager<IdentityUser>> userManagerMock;
    IConfiguration configurationMock = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string> {
            {"JWT:Secret", "vdo81ts87g8vbadm0bz2z4yerxc1dbc6z9g6mllvfgz5m080"},
            {"JWT:ValidAudience", "http://localhost:4200"},
            {"JWT:ValidIssuer", "http://localhost:5000"},
        })
        .Build();

    [SetUp]
    public void Setup()
    {
        userManagerMock = new Mock<UserManager<IdentityUser>>(
            new Mock<IUserStore<IdentityUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<IdentityUser>>().Object,
            new IUserValidator<IdentityUser>[0],
            new IPasswordValidator<IdentityUser>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<IdentityUser>>>().Object
        );
    }

    [Test]
    public void CreateUser_Failed()
    {
        var identityErrors = new IdentityError[]
        {
            new IdentityError()
            {
                Code = "Username already exists",
                Description = "Username already exists"
            }
        };
        
        userManagerMock.Setup(e => e.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(identityErrors));
        userManagerMock.Setup(e => e.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()));
        var service = new UserServices(
            userManagerMock.Object,
            configurationMock
        );

        try
        {
            Task.WaitAll(
                service
                    .CreateUser(new IdentityUser() { UserName = "Test" }, "Test", new IdentityRole() { Name = "Test" },
                        CancellationToken.None
                    )
            );
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.AreEqual(e.Message, "One or more errors occurred. (User creation failed! Please check user details and try again.)");
            userManagerMock.Verify(e => e.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Exactly(1));
            userManagerMock.Verify(e => e.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Exactly(0));
        }
    }
    
    [Test]
    public void CreateUser_Success()
    {
        userManagerMock.Setup(e => e.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(e => e.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()));
        var service = new UserServices(
            userManagerMock.Object,
            configurationMock
        );

        Task.WaitAll(
            service
                .CreateUser(new IdentityUser() { UserName = "Test" }, "Test", new IdentityRole() { Name = "Test" },
                    CancellationToken.None
                )
        );
        userManagerMock.Verify(e => e.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Exactly(1));
        userManagerMock.Verify(e => e.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Exactly(1));
        
    }
    
    [Test]
    public void DeleteUser_Failed()
    {
        userManagerMock.Setup(e => e.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(default(IdentityUser));
        var service = new UserServices(
            userManagerMock.Object,
            configurationMock
        );

        try{
            Task.WaitAll(
            service
                .DeleteUser("Test", CancellationToken.None)
            );
        
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.AreEqual(e.Message, "One or more errors occurred. (User not found.)");
            userManagerMock.Verify(e => e.DeleteAsync(It.IsAny<IdentityUser>()), Times.Exactly(0));
        }
    }
    
    [Test]
    public void DeleteUser_Success()
    {
        userManagerMock.Setup(e => e.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser());
        userManagerMock.Setup(e => e.DeleteAsync(It.IsAny<IdentityUser>()));
        var service = new UserServices(
            userManagerMock.Object,
            configurationMock
        );

        Task.WaitAll(
            service
                .DeleteUser("Test", CancellationToken.None)
        );
    
        userManagerMock.Verify(e => e.DeleteAsync(It.IsAny<IdentityUser>()), Times.Exactly(1));
    }

    [Test]
    public void Login_FailedUserNotFound()
    {
        userManagerMock.Setup(e => e.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(default(IdentityUser));
        var service = new UserServices(
            userManagerMock.Object,
            configurationMock
        );

        try{
            Task.WaitAll(
                service
                    .Login("Test", "Test", CancellationToken.None)
            );
        
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.AreEqual(e.Message, "One or more errors occurred. (User not found)");
            userManagerMock.Verify(e => e.FindByNameAsync(It.IsAny<string>()), Times.Exactly(1));
            userManagerMock.Verify(e => e.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Exactly(0));
            userManagerMock.Verify(e => e.GetRolesAsync(It.IsAny<IdentityUser>()), Times.Exactly(0));
        }
    }
    
    [Test]
    public void Login_FailedInvalidPassword()
    {
        userManagerMock.Setup(e => e.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser());
        userManagerMock.Setup(e => e.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(false);
        var service = new UserServices(
            userManagerMock.Object,
            configurationMock
        );

        try{
            Task.WaitAll(
                service
                    .Login("Test", "Test", CancellationToken.None)
            );
        
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.AreEqual(e.Message, "One or more errors occurred. (Invalid username or password)");
            userManagerMock.Verify(e => e.FindByNameAsync(It.IsAny<string>()), Times.Exactly(1));
            userManagerMock.Verify(e => e.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Exactly(1));
            userManagerMock.Verify(e => e.GetRolesAsync(It.IsAny<IdentityUser>()), Times.Exactly(0));
        }
    }
    
    [Test]
    public void Login_Success()
    {
        userManagerMock.Setup(e => e.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser() { UserName = "Test" });
        userManagerMock.Setup(e => e.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(true);
        userManagerMock.Setup(e => e.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string>() { "Admin" });
        
        var service = new UserServices(
            userManagerMock.Object,
            configurationMock
        );

        Task.WaitAll(
            service
                .Login("Test", "Test", CancellationToken.None)
        );
    
        userManagerMock.Verify(e => e.FindByNameAsync(It.IsAny<string>()), Times.Exactly(1));
        userManagerMock.Verify(e => e.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Exactly(1));
        userManagerMock.Verify(e => e.GetRolesAsync(It.IsAny<IdentityUser>()), Times.Exactly(1));
    }
}