using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AutosarGuiEditor.Source.Render;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Composition;

namespace AutosarGuiEditor.Source.Composition
{
    /// <summary>
    /// Collection of CommentInstance objects within a composition.
    /// </summary>
    public class CommentInstancesList : IGuidList<CommentInstance>
    {
        public CommentInstancesList()
        {
        }

        public void Unselect()
        {
            foreach (CommentInstance comment in this)
            {
                comment.Unselect();
            }
        }

        public bool IsClicked(System.Windows.Point point, out Object clickedObject)
        {
            clickedObject = null;
            foreach (CommentInstance comment in this)
            {
                bool clicked = comment.IsClicked(point, out clickedObject);
                if (clicked == true)
                {
                    return true;
                }
            }

            return false;
        }

        public override void LoadFromXML(XElement xmlApp, String NameId = "")
        {
            XElement xmlList = xmlApp.Element(GetXmlListName() + NameId);
            if (xmlList != null)
            {
                IEnumerable<XElement> elementsList = xmlList.Elements();
                foreach (var element in elementsList)
                {
                    CommentInstance newComment = new CommentInstance();
                    newComment.LoadFromXML(element);
                    base.Add(newComment);
                }
            }
            DoSort();
        }

        public override void WriteToXML(XElement root, String NameId = "")
        {
            XElement listElem = new XElement(GetXmlListName() + NameId);
            foreach (CommentInstance comment in this)
            {
                comment.WriteToXML(listElem);
            }
            root.Add(listElem);
        }

        public override string GetName()
        {
            return "Comment instances";
        }
    }
}
