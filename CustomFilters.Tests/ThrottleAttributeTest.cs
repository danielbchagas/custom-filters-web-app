using System;
using System.Web;
using System.Web.Mvc;
using CustomFilters.App.Filters;
using Moq;
using Xunit;

namespace CustomFilters.Tests
{
    public class ThrottleAttributeTest
    {
        private readonly ThrottleAttribute _throttleAttribute = new ThrottleAttribute();
        
        [Fact]
        public void OnActionExecuting_FirstRequest_ShouldProceed()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContextBase>();
            var requestMock = new Mock<HttpRequestBase>();
            var responseMock = new Mock<HttpResponseBase>();
            var actionExecutingContext = new ActionExecutingContext
            {
                HttpContext = httpContextMock.Object
            };

            requestMock.Setup(r => r.UserHostAddress).Returns("127.0.0.1");
            requestMock.Setup(r => r.Url).Returns(new Uri("http://localhost/test"));
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            // Act
            _throttleAttribute.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.Null(actionExecutingContext.Result);
        }
        
        [Fact]
        public void OnActionExecuting_SecondRequest_ShouldFail()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContextBase>();
            var requestMock = new Mock<HttpRequestBase>();
            var responseMock = new Mock<HttpResponseBase>();
            var actionExecutingContext = new ActionExecutingContext
            {
                HttpContext = httpContextMock.Object
            };

            requestMock.Setup(r => r.UserHostAddress).Returns("127.0.0.1");
            requestMock.Setup(r => r.Url).Returns(new Uri("http://localhost/test"));
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            // Act
            _throttleAttribute.OnActionExecuting(actionExecutingContext);
            
            _throttleAttribute.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsType<HttpStatusCodeResult>(actionExecutingContext.Result);
            var result = (HttpStatusCodeResult)actionExecutingContext.Result;
            Assert.Equal(429, result.StatusCode);
            Assert.Equal("Too Many Requests", result.StatusDescription);
        }
    }
}