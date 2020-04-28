﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Library.Api.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // our binder works only on enumerable types 
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
            //Get the inputted value through the value provider

            var value = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName).ToString();

            // if that value is null or whitespace , we return null

            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // the value isn't null or whitespace,
            // and the type of the model is enumerable.
            // Get the enumerable's type, and a converter
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converer = TypeDescriptor.GetConverter(elementType);

            //convert each item in the value list to the enumerable type
            var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => converer.ConvertFromString(x.Trim()))
                .ToArray();
            // create an array of that type, ans set it as the Model value
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            // Return a successful result, passing  in the Model
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;

        }
    }
}
