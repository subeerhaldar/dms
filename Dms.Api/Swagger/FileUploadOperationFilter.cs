using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dms.Api.Swagger;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath?.Contains("CreateDocument") == true ||
            context.ApiDescription.RelativePath?.Contains("UpdateDocument") == true)
        {
            // Clear the parameters since we're using RequestBody for file upload
            operation.Parameters.Clear();

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["file"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "File to upload"
                                },
                                ["fileName"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "Name of the file"
                                },
                                ["workflowStateId"] = new OpenApiSchema
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Workflow state ID"
                                }
                            },
                            Required = new HashSet<string> { "file" }
                        }
                    }
                }
            };
        }
    }
}