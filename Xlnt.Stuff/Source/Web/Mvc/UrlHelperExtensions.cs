﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Routing;
using System.Web.Mvc;

namespace Xlnt.Web.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string Action(this UrlHelper self, Expression<Action> expr) {
            var method = (MethodCallExpression)expr.Body;
            var routeValues = new RouteValueDictionary();
            var parameters = method.Method.GetParameters();
            for(int i = 0; i != parameters.Length; ++i) {
                var arg = method.Arguments[i];
                routeValues.Add(parameters[i].Name, Value(arg));
            }
            return self.Action(method.Method.Name, routeValues);
        }

        static object Value(Expression expr) {
            switch(expr.NodeType) {
                case ExpressionType.Constant: return (expr as ConstantExpression).Value;
                case ExpressionType.MemberAccess:
                    var memberExpression = expr as MemberExpression;
                    var field = (FieldInfo)memberExpression.Member;
                    return field.GetValue(Value(memberExpression.Expression));
                default: throw new NotSupportedException();
            }
        }
    }
}