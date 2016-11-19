﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    /// <summary>
    /// <see cref="IPageActivator"/> that uses type activation to create Pages.
    /// </summary>
    public class DefaultPageActivator : IPageActivator
    {
        /// <inheritdoc />
        public virtual Func<PageContext, object> Create(CompiledPageActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptor));
            }

            var pageTypeInfo = actionDescriptor.PageTypeInfo?.AsType();
            if (pageTypeInfo == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    nameof(actionDescriptor.PageTypeInfo),
                    nameof(actionDescriptor)),
                    nameof(actionDescriptor));
            }

            return CreatePageFactory(pageTypeInfo);
        }

        /// <inheritdoc />
        public virtual void Release(PageContext context, object page)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            (page as IDisposable)?.Dispose();
        }

        private static Func<PageContext, object> CreatePageFactory(Type pageTypeInfo)
        {
            var parameter = Expression.Parameter(typeof(PageContext), "pageContext");

            // new Page();
            var newExpression = Expression.New(pageTypeInfo);

            // () => new Page();
            var pageFactory = Expression
                .Lambda<Func<PageContext, object>>(newExpression, parameter)
                .Compile();
            return pageFactory;
        }
    }
}