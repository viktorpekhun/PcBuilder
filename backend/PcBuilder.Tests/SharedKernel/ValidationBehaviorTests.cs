using FluentValidation;
using FluentValidation.Results;
using MediatR;
using PcBuilder.SharedKernel.Behaviors;
using ValidationException = PcBuilder.SharedKernel.Exceptions.ValidationException;

namespace PcBuilder.Tests.SharedKernel
{
    public class ValidationBehaviorTests
    {
        private record TestRequest(string Name) : IRequest<string>;

        private class TestRequestValidator : AbstractValidator<TestRequest>
        {
            public TestRequestValidator()
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            }
        }

        [Fact]
        public async Task Handle_WithNoValidators_CallsNext()
        {
            var behavior = new ValidationBehavior<TestRequest, string>(
                Enumerable.Empty<IValidator<TestRequest>>());

            var result = await behavior.Handle(
                new TestRequest("test"),
                () => Task.FromResult("ok"),
                CancellationToken.None);

            Assert.Equal("ok", result);
        }

        [Fact]
        public async Task Handle_WithValidInput_CallsNext()
        {
            var behavior = new ValidationBehavior<TestRequest, string>(
                new[] { new TestRequestValidator() });

            var result = await behavior.Handle(
                new TestRequest("valid name"),
                () => Task.FromResult("ok"),
                CancellationToken.None);

            Assert.Equal("ok", result);
        }

        [Fact]
        public async Task Handle_WithInvalidInput_ThrowsValidationException()
        {
            var behavior = new ValidationBehavior<TestRequest, string>(
                new[] { new TestRequestValidator() });

            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(
                    new TestRequest(""),
                    () => Task.FromResult("ok"),
                    CancellationToken.None));

            Assert.Single(exception.Errors);
            Assert.Contains("Name is required.", exception.Errors);
        }

        private class AlwaysFailValidator1 : AbstractValidator<TestRequest>
        {
            public AlwaysFailValidator1()
            {
                RuleFor(x => x.Name).Must(_ => false).WithMessage("Error 1");
            }
        }

        private class AlwaysFailValidator2 : AbstractValidator<TestRequest>
        {
            public AlwaysFailValidator2()
            {
                RuleFor(x => x.Name).Must(_ => false).WithMessage("Error 2");
            }
        }

        [Fact]
        public async Task Handle_WithMultipleValidators_AggregatesErrors()
        {
            var behavior = new ValidationBehavior<TestRequest, string>(
                new IValidator<TestRequest>[] { new AlwaysFailValidator1(), new AlwaysFailValidator2() });

            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(
                    new TestRequest("test"),
                    () => Task.FromResult("ok"),
                    CancellationToken.None));

            // Both validators should produce at least one error each
            Assert.True(exception.Errors.Count >= 2,
                $"Expected at least 2 errors but got {exception.Errors.Count}: [{string.Join(", ", exception.Errors)}]");
            Assert.Contains("Error 1", exception.Errors);
            Assert.Contains("Error 2", exception.Errors);
        }
    }
}
