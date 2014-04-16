using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Flatten.Core
{

    /// <summary>
    /// Flattens XDocument instances
    /// </summary>
    public static class Flattener
    {

        public static char[] DefaultDelimiters
        {
            get { return new[] { ';', ',', ' ' }; }
        }

        /// <summary>
        /// Creates a new instance of XDocument where each selected element from <paramref name="xpath"/> 
        /// is replaced by one or more copies for each token contained in the value of <paramref name="child"/>.
        /// Tokens are delimited by ';', ',', and ' ' charactes.
        /// </summary>
        /// <param name="src">Source XDocument instance</param>
        /// <param name="xpath">XPath expression to select elements containing composite values</param>
        /// <param name="child">Name of the child elemement of elements returned by <paramref name="xpath"/>, potentially containing composite values</param>
        /// <returns>New XDocument instance</returns>
        public static XDocument Flatten(XDocument src, string xpath, XName child)
        {
            return Flatten(src, xpath, child, DefaultDelimiters);
        }

        /// <summary>
        /// Creates a new instance of XDocument where each selected element from <paramref name="xpath"/> 
        /// is replaced by one or more copies for each token contained in the value of <paramref name="child"/>.
        /// </summary>
        /// <param name="src">Source XDocument instance</param>
        /// <param name="xpath">XPath expression to select elements containing composite values</param>
        /// <param name="child">Name of the child elemement of elements returned by <paramref name="xpath"/>, potentially containing composite values</param>
        /// <param name="delimiters">sequence of delimiting charaters</param>
        /// <returns>New XDocument instance</returns>
        public static XDocument Flatten(XDocument src, string xpath, XName child, IEnumerable<char> delimiters)
        {
            var local = new XDocument(src);
            var delimitersSet = new HashSet<char>(delimiters);

            foreach (var element in local.XPathSelectElements(xpath)
                                         .Where(element => element.Element(child) != null)
                                         .Where(element =>
                                             delimitersSet.Any(delimiter => element.Element(child).Value.Contains(delimiter)))
                                         .ToList())
            {
                var unpacked = Unpack(element, child, delimitersSet.ToArray());
                element.AddAfterSelf(unpacked);
                element.Remove();
            }

            return local;
        }

        private static IEnumerable<XElement> Unpack(XElement element, XName child, char[] delimiters)
        {
            return element.Element(child).Value
                                         .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(value =>
                                         {
                                             var split = new XElement(element);
                                             split.Element(child).Value = value;
                                             return split;
                                         });
        }

    }

}
