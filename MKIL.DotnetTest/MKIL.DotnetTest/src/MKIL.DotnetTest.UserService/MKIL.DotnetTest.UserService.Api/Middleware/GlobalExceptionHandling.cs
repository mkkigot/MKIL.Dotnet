using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Diagnostics;

namespace MKIL.DotnetTest.UserService.Api.Middleware
{
    public class GlobalExceptionHandling : IExceptionFilter
    {
        public GlobalExceptionHandling()
        {

        }

        public void OnException(ExceptionContext context)
        {
            throw new NotImplementedException();
        }
        /*
public void OnException(ExceptionContext context)
{
   var stackTrace = new StackTrace(context.Exception, true);
   foreach (var frame in stackTrace.GetFrames())
   {
       var fileName = frame.GetFileName();
       var method = frame.GetMethod();
       var lineNumber = frame.GetFileLineNumber();

       if (!string.IsNullOrEmpty(fileName) && lineNumber != 0)
       {
           // Handle async method state machines (MoveNext)
           var methodName = method.DeclaringType?.Name;
           if (methodName == "MoveNext" && method.DeclaringType?.DeclaringType != null)
           {
               // Get the original async method name by reflecting on the declaring type
               methodName = method.DeclaringType.DeclaringType.Name;
           }
           else if (methodName != null && methodName.Contains(">"))
           {
               // Handle async/iterator method names generated with a pattern like "<MethodName>d__"
               methodName = methodName.Substring(1, methodName.IndexOf(">") - 1);
           }

           // todo: get correlationId here
           Log.Error(context.Exception, methodName, fileName, lineNumber);
           LogError($"Error: {context.Exception}", $"{refId}", methodName, fileName, lineNumber)

           var problemDetails = new ProblemDetails
           {
               Status = StatusCodes.Status500InternalServerError,
               Title = $"Internal Server Error. Please contact ITGBO and provide this exception reference : {correlationId}",
               Detail = context.Exception.Message,
               Instance = context.HttpContext.Request.Path
           };

           context.Result = new ObjectResult(problemDetails)
           {
               StatusCode = StatusCodes.Status500InternalServerError
           };
      }
      else
      {
          var refId = Guid.NewGuid();
          WatchLogger.LogError($"Error: {context.Exception}", $"{refId}", "", fileName, lineNumber);

          var problemDetails = new ProblemDetails
          {
              Status = StatusCodes.Status500InternalServerError,
              Title = $"Internal Server Error. Please contact ITGBO and provide this exception reference : {refId}",
              Detail = context.Exception.Message,
              Instance = context.HttpContext.Request.Path
          };

          context.Result = new ObjectResult(problemDetails)
          {
              StatusCode = StatusCodes.Status500InternalServerError
          };
      }

       context.ExceptionHandled = true;
   }
}

*/
    }
}
