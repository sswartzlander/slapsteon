﻿using System;
using System.Web.Mvc;

namespace Slapsteon.RazorWeb.Business
{
    class DeviceModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            var typeValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".ModelType");
            var type = Type.GetType((string)typeValue.ConvertTo(typeof(string)), true);

            var model = Activator.CreateInstance(type);
            bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, type);
            return model;
        }
    }
}