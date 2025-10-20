using FluentValidation.TestHelper;
using GameStore.Application.Features.Users.DTOs;
using GameStore.Application.Features.Users.Validators;
using Xunit;

namespace GameStore.Tests.Application.Validators;

public class RegisterRequestValidatorTests
{
  private readonly RegisterRequestValidator _validator;

  public RegisterRequestValidatorTests()
  {
    _validator = new RegisterRequestValidator();
  }

  [Fact]
  public void Should_Have_Error_When_Name_Is_Empty()
  {
    // Arrange
    var request = new RegisterRequest("", "test@example.com", "testuser", "Password123!");

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Name)
      .WithErrorMessage("Nome é obrigatório");
  }

  [Fact]
  public void Should_Have_Error_When_Name_Is_Too_Short()
  {
    // Arrange
    var request = new RegisterRequest("A", "test@example.com", "testuser", "Password123!");

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Name)
      .WithErrorMessage("Nome deve ter no mínimo 2 caracteres");
  }

  [Fact]
  public void Should_Have_Error_When_Email_Is_Invalid()
  {
    // Arrange
    var request = new RegisterRequest("Test User", "invalid-email", "testuser", "Password123!");

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Email)
      .WithErrorMessage("Email inválido");
  }

  [Fact]
  public void Should_Have_Error_When_Password_Is_Too_Short()
  {
    // Arrange
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Pass1!");

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Password)
      .WithErrorMessage("Senha deve ter no mínimo 8 caracteres");
  }

  [Fact]
  public void Should_Have_Error_When_Password_Missing_Uppercase()
  {
    // Arrange
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "password123!");

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Password)
      .WithErrorMessage("Senha deve conter pelo menos uma letra maiúscula");
  }

  [Fact]
  public void Should_Have_Error_When_Username_Contains_Invalid_Characters()
  {
    // Arrange
    var request = new RegisterRequest("Test User", "test@example.com", "test@user", "Password123!");

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Username)
      .WithErrorMessage("Username deve conter apenas letras, números e underscore");
  }

  [Fact]
  public void Should_Not_Have_Error_When_Request_Is_Valid()
  {
    // Arrange
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Password123!");

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Should_Stop_At_First_Error_When_Multiple_Validations_Fail()
  {
    // Arrange - Email vazio (primeira validação falha)
    var request = new RegisterRequest("Test User", "", "testuser", "Password123!");

    // Act
    var result = _validator.TestValidate(request);

    // Assert - Deve parar na primeira falha (NotEmpty) e não validar EmailAddress
    var emailErrors = result.Errors.Where(e => e.PropertyName == "Email").ToList();
    Assert.Single(emailErrors);
    Assert.Equal("Email é obrigatório", emailErrors[0].ErrorMessage);
  }
}

