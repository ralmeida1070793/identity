using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using Identity.Controllers;
using Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IdentityTests.ControllerTests;

public class UserControllerTests
{
    private Mock<IRoleServices> roleServiceMock = new Mock<IRoleServices>();
    private Mock<IUserServices> userServiceMock = new Mock<IUserServices>();
    
    [SetUp]
    public void Setup()
    {
        roleServiceMock
            .Setup(e => e.GetRole(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityRole()
            {
                Name = "Test"
            });

        userServiceMock
            .Setup(e => e.Login(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new JwtSecurityToken());
        
        userServiceMock
            .Setup(e => e.CreateUser(
                It.IsAny<IdentityUser>(), 
                It.IsAny<string>(),
                It.IsAny<IdentityRole>(), 
                It.IsAny<CancellationToken>())
            );
    }
    
    
    [Test]
    public void Login_Success()
    {
        var controller = new UserController(userServiceMock.Object, roleServiceMock.Object);
        controller.Login(
            new LoginModel()
            {
                Username = "Test",
                Password = "Test"
            }, 
            CancellationToken.None
        );
        
        userServiceMock.Verify(r => r.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
    }

    [Test]
    public void Register_UserExists()
    {
        userServiceMock
            .Setup(e => e.GetUserByName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUser() { UserName = "Test" });
        
        var controller = new UserController(userServiceMock.Object, roleServiceMock.Object);
        var result = controller.Register(
            new RegisterModel()
            {
                Username = "Test",
                Password = "Test",
                Email = "Test@test.com",
                Role = "Test"
            },
            CancellationToken.None
        ).Result as ObjectResult;
        
        Assert.IsTrue(result.Value.Equals("User already exists!"));        
    }
    
    [Test]
    public void Register_Success()
    {
        userServiceMock
            .Setup(e => e.GetUserByName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IdentityUser));
        
        var controller = new UserController(userServiceMock.Object, roleServiceMock.Object);
        controller.Register(
            new RegisterModel()
            {
                Username = "Test",
                Password = "Test",
                Email = "Test@test.com",
                Role = "Test"
            },
            CancellationToken.None
        );
        
        userServiceMock.Verify(r => r.CreateUser(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<IdentityRole>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
    }

    [Test]
    public void GetUserRoles_UserNotFound()
    {
        userServiceMock
            .Setup(e => e.GetUserByName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(IdentityUser));
        
        var controller = new UserController(userServiceMock.Object, roleServiceMock.Object);
        var result = controller.GetUserRoles(
            "Test",
            CancellationToken.None
        ).Result as ObjectResult;
        
        Assert.IsTrue(result.Value.Equals("User not found"));
    }
    
    [Test]
    public void GetUserRoles_Success()
    {
        userServiceMock
            .Setup(e => e.GetUserByName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUser());

        userServiceMock
            .Setup(e => e.GetUserRoles(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>()
            {
                "Test",
                "Test1"
            });
        
        var controller = new UserController(userServiceMock.Object, roleServiceMock.Object);
        controller.GetUserRoles(
            "Test",
            CancellationToken.None
        );
        
        userServiceMock.Verify(r => r.GetUserRoles(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
    }
}