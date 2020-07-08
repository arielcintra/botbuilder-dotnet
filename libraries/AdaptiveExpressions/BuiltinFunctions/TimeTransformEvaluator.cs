﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Evaluator that transforms a date-time to another date-time.
    /// </summary>
    public class TimeTransformEvaluator : ExpressionEvaluator
    {
        public TimeTransformEvaluator(string type, Func<DateTime, int, DateTime> function)
            : base(type, Evaluator(function), ReturnType.String, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String, ReturnType.String }, ReturnType.String, ReturnType.Number);
        }

        private static EvaluateExpressionDelegate Evaluator(Func<DateTime, int, DateTime> function)
        {
            return (expression, state, options) =>
            {
                object value = null;
                string error = null;
                IReadOnlyList<object> args;
                var locale = options.Locale != null ? new CultureInfo(options.Locale) : Thread.CurrentThread.CurrentCulture;
                var format = FunctionUtils.DefaultDateTimeFormat;
                (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
                if (error == null)
                {
                    (format, locale, error) = FunctionUtils.DetermineFormatAndLocale(args, format, locale, 4);
                }

                if (error == null)
                {
                    if (args[1].IsInteger())
                    {
                        (value, error) = FunctionUtils.NormalizeToDateTime(args[0], dt => function(dt, Convert.ToInt32(args[1])).ToString(format, locale));
                    }
                    else
                    {
                        error = $"{expression} should contain an ISO format timestamp and a time interval integer.";
                    }
                }

                return (value, error);
            };
        }
    }
}