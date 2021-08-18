using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Widely.DataModel.ViewModels.Common
{

    public class SearchCriteria<T>
    {
        public T criteria { get; set; }
        public GridCriteria gridCriteria { get; set; } = null;
    }

    public class GridCriteria
    {
        public int pageSize { get; set; }
        public int page { get; set; }
        public string sortby { get; set; }
        public string sortdir { get; set; }
        public int skip { get { return (pageSize * (page - 1)); } }
        public int Take { get { return (pageSize); } }
        public int totalRecord { get; set; }
        public int totalPage { get; set; }
    }

    public class Sort
    {
        public string field { get; set; }
        public string dir { get; set; }
    }


    public class GridResult<T>
    {
        public IEnumerable<T> Items { get; set; }

        public Pagination pagination { get; set; }
    }

    public class Pagination
    {
        public int pageSize { get; set; }
        public int page { get; set; }
        public int totalRecord { get; set; }
        public int totalPage { get; set; }
        public string sortby { get; set; }
        public string sortdir { get; set; }
    }

    public static class OrderExpression
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string memberName)
        {
            ParameterExpression[] typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };

            System.Reflection.PropertyInfo pi = typeof(T).GetProperty(memberName);

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                typeof(Queryable),
                "OrderBy",
                new Type[] { typeof(T), pi.PropertyType },
                query.Expression,
                Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string memberName)
        {
            ParameterExpression[] typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };

            System.Reflection.PropertyInfo pi = typeof(T).GetProperty(memberName);

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                typeof(Queryable),
                "OrderByDescending",
                new Type[] { typeof(T), pi.PropertyType },
                query.Expression,
                Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }

        public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> list, string memberName)
        {
            PropertyInfo prop = typeof(T).GetProperty(memberName);

            if (prop == null)
            {
                throw new Exception("No property '" + memberName + "' in + " + typeof(T).Name + "'");
            }

            return list.OrderBy(x => prop.GetValue(x, null));
        }

        public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> list, string memberName)
        {
            PropertyInfo prop = typeof(T).GetProperty(memberName);

            if (prop == null)
            {
                throw new Exception("No property '" + memberName + "' in + " + typeof(T).Name + "'");
            }

            return list.OrderByDescending(x => prop.GetValue(x, null));
        }

    }
}
