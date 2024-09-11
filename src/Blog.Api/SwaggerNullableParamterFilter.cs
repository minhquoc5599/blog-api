﻿using AutoMapper.Internal;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Blog.Api
{
    public class SwaggerNullableParamterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (!parameter.Schema.Nullable
                && (context.ApiParameterDescription.Type.IsNullableType()
                || !context.ApiParameterDescription.Type.IsValueType))
            {
                parameter.Schema.Nullable = true;
            }
        }
    }
}
