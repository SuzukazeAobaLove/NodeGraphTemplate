using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MikanLab.NodeGraph
{
    public static class EditorUtilities
    {
        #region 对应类型查询
        private static Dictionary<Type, Type> nodeDrawers;
        private static Dictionary<Type, Type> graphViews;
        private static Dictionary<Type, Type> graphWindows;

        public static Type GetNodeDrawers(Type type)
        {
            if (!type.IsSubclassOf(typeof(BaseNode))) throw new Exception("Invalid Query!");
            if (nodeDrawers == null) InitDict();
            while (!nodeDrawers.ContainsKey(type))
            {
                if(type.BaseType != null) type = type.BaseType;
                else
                {
                    type = typeof(BaseNode);
                    break;
                }
            }
            return nodeDrawers[type];
        }

        public static Type GetGraphView(Type type)
        {
            if (!type.IsAssignableTo(typeof(NodeGraph))) throw new Exception("Invalid Query!");
            if (graphViews == null) InitDict();
            while (!graphViews.ContainsKey(type))
            {
                if (type.BaseType != null) type = type.BaseType;
                else
                {
                    type = typeof(NodeGraph);
                    break;
                } 
            }
            return graphViews[type];
        }

        public static Type GetGrpahWindow(Type type)
        {
            if (!type.IsAssignableTo(typeof(NodeGraph))) throw new Exception("Invalid Query!");
            if (graphWindows == null) InitDict();
            while (!graphWindows.ContainsKey(type))
            {
                if (type.BaseType != null) type = type.BaseType;
                else
                {
                    type = typeof(NodeGraph);
                    break;
                }
            }
            return graphWindows[type];
        }

        /// <summary>
        /// 能否将当前类实例赋值给目标类引用
        /// </summary>
        /// <param name="origin">当前类</param>
        /// <param name="target">目标类</param>
        /// <returns></returns>
        public static bool IsAssignableTo(this Type origin, Type target)
        {
            return target.IsAssignableFrom(origin);
        }

        private static void InitDict()
        {
            nodeDrawers = new();
            graphWindows = new();
            graphViews = new();
            nodeDrawers.Add(typeof(BaseNode), typeof(NodeDrawer));
            graphWindows.Add(typeof(NodeGraph),typeof(NodeGraphWindow));
            graphViews.Add(typeof(NodeGraph),typeof(NodeGraphView));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    //处理视图
                    if (type.IsSubclassOf(typeof(NodeGraphView)))
                    {
                        var attr = type.GetCustomAttributes(typeof(CustomGraphViewAttribute), false) as CustomGraphViewAttribute[];
                        if(attr == null || attr.Length == 0) continue;
                        else
                        {
                            graphViews.Add(attr[0].Type, type);
                        }
                    }
                    //处理绘制器
                    else if (type.IsSubclassOf(typeof(NodeDrawer)))
                    {
                        var attr = type.GetCustomAttributes(typeof(GraphDrawerAttribute), false) as GraphDrawerAttribute[];
                        if (attr == null || attr.Length == 0) continue;
                        else
                        {
                            nodeDrawers.Add(attr[0].Type, type);
                        }
                    }
                    //处理窗口
                    else if(type.IsSubclassOf(typeof(NodeGraphWindow)))
                    {
                        var attr = type.GetCustomAttributes(typeof(CustomGraphWindowAttribute), false) as CustomGraphWindowAttribute[];
                        if (attr == null || attr.Length == 0) continue;
                        else
                        {
                            graphWindows.Add(attr[0].Type, type);
                        }
                    }
                }
            }
        }
        #endregion

        #region 处理资源打开
        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenGraph(int instanceID,int linenumber)
        {
            string path = AssetDatabase.GetAssetPath(instanceID);
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

            if(type == typeof(NodeGraph) || type.IsSubclassOf(typeof(NodeGraph)))
            {
                var windowtype = EditorUtilities.GetGrpahWindow(type);
                windowtype.InvokeMember("Invoke", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod, null, null,
                    new object[] { AssetDatabase.LoadAssetAtPath<NodeGraph>(path) });
                return true;
            }
            return false;
        }
        #endregion
    }
}