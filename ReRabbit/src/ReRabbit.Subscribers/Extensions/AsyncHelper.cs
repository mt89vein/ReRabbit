using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ReRabbit.Subscribers.Extensions
{
    /// <summary>
    /// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
    /// Licensed under the MIT License, Version 2.0. See License.txt in the project root for license information.
    /// https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default
        );

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            return _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }
    }
}