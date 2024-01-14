using NRPUtils.MVVMBase;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NRPUtils.Model
{
    public class Node<T> : NotifyObject,IEnumerable,IEnumerator
    {
        private T      item;
        private bool   selected;
        private Queue  queue;
        private object current;

        public string                        Name     { get; set; }
        public ObservableCollection<Node<T>> Nodes    { get; set; } = new ObservableCollection<Node<T>>();
        public T                             Item     { get => item; set { item = value; OnPropertyChanged(); } }
        public bool                          Selected { get => selected; set { selected = value; OnPropertyChanged(); } }

        public List<Node<T>> GetSelectedSubNodes()
        {
            var list = new List<Node<T>>();
            var queue = new Queue();
            queue.Enqueue(this);
            while (queue.Count> 0)
            {
                var node = (Node<T>)queue.Dequeue();
                if (node.Item!=null && node.Selected) list.Add(node);
                if (node.Nodes.Count> 0)
                {
                    foreach (var item in node.Nodes)
                    {
                        queue.Enqueue(item);
                    }
                }
            }
            return list;
        }
        public Node<T> FindSubNodeByName(string Name)
        {
            var queue = new Queue();
            queue.Enqueue(this);
            while (queue.Count> 0)
            {
                var node = (Node<T>)queue.Dequeue();
                if (node.Name == Name) return node;
                if (node.Nodes.Count> 0)
                {
                    foreach (var item in node.Nodes)
                    {
                        queue.Enqueue(item);
                    }
                }
            }
            return null;
        }
        public void Selected_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName!=nameof(Selected)) return;
            Selected=((Node<T>)sender).Selected;
        }

        
        #region IEnumerable,IEnumerator
        public object Current => current;
        
        public bool MoveNext()
        {
            while (queue.Count>0)
            {
                current=queue.Dequeue();
                if (!(current is Node<T> n)) return false;
                if (n.Nodes.Count>0)
                {
                    foreach (var item in n.Nodes)
                    {
                        queue.Enqueue(item);
                    }
                }
                return true;
            }
            return false;
        }

        public void Reset()
        {
            queue = new Queue();
            queue.Enqueue(this);
        }

        public IEnumerator GetEnumerator()
        {
            Reset();
            return this;
        }
        #endregion
    }
}
