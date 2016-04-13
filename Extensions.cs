using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.CSharp.RuntimeBinder;

namespace System
{
    public static class Extensions
    {
        /// <summary>
        /// returns a dynamic object property value by property name - from StackOverflow
        /// </summary>
        public static object GetPropertyValue(this DynamicJsonObject o, string member)
        {
            if (o == null) throw new ArgumentNullException("o");
            if (member == null) throw new ArgumentNullException("member");
            Type scope = o.GetType();
            var provider = o as IDynamicMetaObjectProvider;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (provider != null)
            {
                //Magic
                ParameterExpression param = Expression.Parameter(typeof(object));
                DynamicMetaObject mobj = provider.GetMetaObject(param);
                GetMemberBinder binder = (GetMemberBinder)Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, member, scope, new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(0, null) });
                DynamicMetaObject ret = mobj.BindGetMember(binder);
                BlockExpression final = Expression.Block(
                    Expression.Label(CallSiteBinder.UpdateLabel),
                    ret.Expression
                );
                LambdaExpression lambda = Expression.Lambda(final, param);
                Delegate del = lambda.Compile();
                return del.DynamicInvoke(o);
            }
            else {
                return o.GetType().GetProperty(member, BindingFlags.Public | BindingFlags.Instance).GetValue(o, null);
            }
        }

        /// <summary>
        /// Converts a dictionary of field value pairs to a concrete object, auto maps to uppercase field if lower case field is not present
        /// </summary>
        public static T ToObject<T>(this IDictionary<string, object> source) where T : class, new()
        {
            T someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (KeyValuePair<string, object> item in source)
            {
                var prop = someObjectType.GetProperty(item.Key);
                if (prop == null && !char.IsUpper(item.Key[0])) // check if the first letter is uppercase 
                {
                    var capitalizedKey = item.Key.First().ToString().ToUpper() + String.Join("", item.Key.Skip(1));
                    prop = someObjectType.GetProperty(capitalizedKey);
                }

                prop?.SetValue(someObject, item.Value, null);
            }

            return someObject;
        }
    }
}
