using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Interfaces
{
    public class IGuidList<T> : List<T>, IAutosarTreeList where T : IGUID, new()
    {
        public T FindObject(IGUID iguid)
        {
            Guid guid = iguid.GUID;
            return FindObject(guid);
        }

        public T FindObject(Guid guid)
        {
            foreach (T obj in this)
            {
                object guidProperty = GetProperty(obj, "GUID");
                if (guidProperty != null)
                {
                    Guid propGuid = (Guid)guidProperty;
                    if (propGuid.CompareTo(guid) == 0) /* 0 - means that they are equal */
                    {
                        return obj;
                    }
                }          
            }
            return default(T);
        }

        public T FindObject(String Name)
        {
            foreach (T obj in this)
            {
                object guidProperty = GetProperty(obj, "Name");
                if (guidProperty != null)
                {
                    String propName = guidProperty.ToString();
                    if (propName.CompareTo(Name) == 0) /* 0 - means that they are equal */
                    {
                        return obj;
                    }
                }          
            }
            return default(T);
        }

        protected object GetProperty(Object obj, String propertyName)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                if (property.Name == propertyName)
                {
                    return property.GetValue(obj);
                }
            }
            return null;
        }

        public virtual String GetXmlListName()
        {
            return this.GetType().Name;
        }
        
        public new void Add(T item)
        {
            base.Add(item);
            DoSort();
        }

        public virtual void LoadFromXML(XElement xmlApp, String NameId = "")
        {
            XElement xmlList = xmlApp.Element(GetXmlListName() + NameId);
            if (xmlList != null)
            {
                IEnumerable<XElement> elementsList = xmlList.Elements();
                if (elementsList != null)
                {
                    foreach (var element in elementsList)
                    {
                        T newObj = new T();
                        newObj.LoadFromXML(element);
                        base.Add(newObj);
                    }
                }
                DoSort();
            }
        }

        public virtual void WriteToXML(XElement root, String NameId = "")
        {
            XElement dtList = new XElement(GetXmlListName() + NameId);
            foreach (T obj in this)
            {
                obj.WriteToXML(dtList);
            }
            root.Add(dtList);
        }

        /* Sort elements in list by its names */
        public virtual void DoSort()
        {
            Sort(delegate(T x, T y)
            {
                object namePropertyX = GetProperty(x, "Name");
                object namePropertyY = GetProperty(y, "Name");
                if ((namePropertyX != null) && (namePropertyY != null))
                {
                    string xName = namePropertyX.ToString();
                    string yName = namePropertyY.ToString();

                    if (xName == null && yName == null) return 0;
                    else if (xName == null) return -1;
                    else if (yName == null) return 1;
                    else return xName.CompareTo(yName);
                }

                throw new Exception("Sort exception! Properties not exists!");                
            });
        }

        public virtual void SortByField(String fieldName)
        {
            Sort(delegate(T x, T y)
            {
                object namePropertyX = GetProperty(x, fieldName);
                object namePropertyY = GetProperty(y, fieldName);
                if ((namePropertyX != null) && (namePropertyY != null))
                {
                    string xName = namePropertyX.ToString();
                    string yName = namePropertyY.ToString();

                    if (xName == null && yName == null) return 0;
                    else if (xName == null) return -1;
                    else if (yName == null) return 1;
                    else return xName.CompareTo(yName);
                }

                throw new Exception("Sort exception! Properties not exists!");
            });
        }

        public virtual String GetName()
        {
            return "IGuidList";
        }

        public List<IGUID> GetItems()
        {
            List<IGUID> list = new List<IGUID>();
            foreach (T def in this)
            {
                list.Add(def);
            }
            return list;
        }

        public virtual Boolean CreateRoot()
        {
            return true;
        }

        public void MoveItemUp(int index)
        {
            if ((index > 0) && (index < Count))
            {
                T item = this[index];
                RemoveAt(index);
                Insert(index - 1, item);
            }
        }

        public void MoveItemDown(int index)
        {
            if ((index >= 0) && (index < Count - 1))
            {
                T item = this[index];
                RemoveAt(index);
                Insert(index + 1, item);
            }
        }
    }
}
