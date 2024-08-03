using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CustomFilters.App.Filters
{
    public class ThrottleAttribute : ActionFilterAttribute
    {
        private static readonly Dictionary<string, DateTime> LastRequestTimes = new Dictionary<string, DateTime>();
        private static readonly TimeSpan ThrottleDuration = TimeSpan.FromMinutes(1);
        private const int TooManyRequestsStatusCode = 429;
        private const string TooManyRequestsStatusMessage = "Too Many Requests";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;
            
            var key = request.UserHostAddress + "-" + request.Url.AbsolutePath;

            lock (LastRequestTimes)
            {
                if (LastRequestTimes.TryGetValue(key, out var lastRequestTime))
                {
                    var currentTime = DateTime.Now;
                    if (currentTime - lastRequestTime < ThrottleDuration)
                    {
                        filterContext.Result = new HttpStatusCodeResult(TooManyRequestsStatusCode, TooManyRequestsStatusMessage);
                        
                        response.Write($@"
                            <script>
                                let confirmed = confirm('Error: {TooManyRequestsStatusMessage}');
                                if (confirmed) {{
                                    window.location = '/';
                                }}
                                else {{
                                    window.location = '/home/error';
                                }}
                            </script>");
                        
                        return;
                    }
                }

                LastRequestTimes[key] = DateTime.Now;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}