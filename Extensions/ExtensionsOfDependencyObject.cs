using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BrightnessChanger
{
    public static class ExtensionsOfDependencyObject
    {
        public static IEnumerable<T> VisualTreeChildren<T>(this DependencyObject element)
        {
            var childs = VisualTreeHelper.GetChildrenCount(element);
            for (var i = 0; i <= childs - 1; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (typeof(T).IsAssignableFrom(child.GetType()))
                    yield return (dynamic)child;
                else
                    foreach (var e in child.VisualTreeChildren<T>())
                        yield return e;
            }
        }
    }
}
