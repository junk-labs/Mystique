using System;
using System.Collections.Generic;
using System.Reflection;
using Inscribe.Filter;

namespace Inscribe.Storage
{
    /// <summary>
    /// フィルタストレージ
    /// </summary>
    public static class FilterStorage
    {
        static FilterStorage() { }

        /// <summary>
        /// レジストされたフィルタ
        /// </summary>
        private static List<Type> filterTypes = new List<Type>();

        private static List<Tuple<Type, string>> filterLookup = new List<Tuple<Type, string>>();

        /// <summary>
        /// 登録されているフィルタ型一覧を取得します。
        /// </summary>
        public static IEnumerable<Type> RegisteredFilters
        {
            get { return filterTypes; }
        }

        /// <summary>
        /// 指定した名前空間とクラスを持つフィルタ型を取得します。
        /// </summary>
        /// <param name="ns">名前空間</param>
        /// <param name="cls">クラス名</param>
        /// <returns>名前空間とクラス名が一致する型</returns>
        public static IEnumerable<Type> GetFilterFromNsAndClass(string identifier)
        {
            foreach (var fl in filterLookup)
            {
                if (fl.Item2 == identifier)
                    yield return fl.Item1;
            }
        }

        /// <summary>
        /// フィルタシステムにエレメントオブジェクトを登録します。
        /// </summary>
        /// <param name="type">登録する型(public T : Athenaeum.Filter.FilterBase)</param>
        /// <returns>登録されればtrue</returns>
        public static bool RegisterFilter(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!type.IsPublic)
            {
                throw new ArgumentException("登録するフィルタはパブリッククラスである必要があります。");
            }
            if (!type.IsSubclassOf(typeof(FilterBase)))
            {
                throw new ArgumentException("登録するフィルタはフィルタベースクラス(Athenaeum.Filter.FilterBase)から派生している必要があります。");
            }
            if (type.IsAbstract)
            {
                throw new ArgumentException("抽象クラスフィルタは登録できません。");
            }
            if (type.IsGenericType)
            {
                throw new ArgumentException("ジェネリッククラスフィルタは登録できません。");
            }
            if (!filterTypes.Contains(type))
            {
                var inst = Activator.CreateInstance(type) as FilterBase;
                if (inst == null)
                    throw new ArgumentException("フィルタを作成できません。");
                System.Diagnostics.Debug.WriteLine("Join:" + type.ToString());
                filterTypes.Add(type);
                filterLookup.Add(new Tuple<Type, string>(type, inst.Identifier));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// フィルタシステムにエレメントオブジェクトを登録します。
        /// </summary>
        /// <param name="type">登録する型(Athenaeum.Filter.FilterBase)</param>
        /// <returns>登録されればtrue</returns>
        public static bool RegisterFilter<T>()
            where T : FilterBase
        {
            return RegisterFilter(typeof(T));
        }

        /// <summary>
        /// 実行中のアセンブリに存在するすべてのフィルタを列挙し、レジストラントに追加します。
        /// </summary>
        public static void RegisterAllFiltersInAsm(Assembly asm)
        {

            foreach (var type in asm.GetTypes())
            {
                if (type.IsSubclassOf(typeof(FilterBase)))
                {
                    try
                    {
                        RegisterFilter(type);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}
