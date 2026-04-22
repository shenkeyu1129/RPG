using System.Collections.Generic;
using System;

public class EventCenter<T> where T : Enum
{
   private readonly Dictionary<T,Delegate> _eventTable = new();

    #region 无参数事件
   // 添加监听
   public void AddListener(T eventType,Action listener)
   {
      if(listener == null)return;
      if(!_eventTable.ContainsKey(eventType))
        {
            _eventTable[eventType] = listener;
        }
        else
        {
            _eventTable[eventType] = Delegate.Combine(_eventTable[eventType], listener);
        }
   }

    // 移除监听
   public void RemoveListener(T eventType,Action listener)
   {
      if(listener == null)return;
      if(_eventTable.TryGetValue(eventType, out var del))
        {
            var newDel = Delegate.Remove(del, listener);
            if(newDel == null)
            {
                _eventTable.Remove(eventType);
            }
            else
            {
                _eventTable[eventType] = newDel;
            }
        }
   }

   // 触发监听
   public void Trigger(T eventType)
    {
        if(_eventTable.TryGetValue(eventType, out var del))
        {
            (del as Action)?.Invoke();
        }
    }
   #endregion
    #region 1个参数事件
   // 添加监听
   public void AddListener<T1>(T eventType,Action<T1> listener)
    {
        if(listener == null)return;
        if(!_eventTable.ContainsKey(eventType))
        {
            _eventTable[eventType] = listener;
        }
        else
        {
            _eventTable[eventType] = Delegate.Combine(_eventTable[eventType], listener);
        }
    }

    // 移除监听
    public void RemoveListener<T1>(T eventType,Action<T1> listener)
    {
        if(listener == null)return;
        if(_eventTable.TryGetValue(eventType, out var del))
        {
            var newDel = Delegate.Remove(del, listener);
            if(newDel == null)
            {
                _eventTable.Remove(eventType);
            }
            else
            {
                _eventTable[eventType] = newDel;
            }
        }
    }

    // 触发监听
    public void Trigger<T1>(T eventType,T1 arg1)
    {
        if(_eventTable.TryGetValue(eventType, out var del))
        {
            (del as Action<T1>)?.Invoke(arg1);
        }
    }
    #endregion
    #region 2个参数事件
    // 添加监听
    public void AddListener<T1,T2>(T eventType,Action<T1,T2> listener)
    {
        if(listener == null)return;
        if(!_eventTable.ContainsKey(eventType))
        {
            _eventTable[eventType] = listener;
        }
        else
        {
            _eventTable[eventType] = Delegate.Combine(_eventTable[eventType], listener);
        }
    }

    // 移除监听
    public void RemoveListener<T1,T2>(T eventType,Action<T1,T2> listener)
    {
        if(listener == null)return;
        if(_eventTable.TryGetValue(eventType, out var del))
        {
            var newDel = Delegate.Remove(del, listener);
            if(newDel == null)
            {
                _eventTable.Remove(eventType);
            }
            else
            {
                _eventTable[eventType] = newDel;
            }
        }
    }

    // 触发监听
    public void Trigger<T1,T2>(T eventType,T1 arg1,T2 arg2)
    {
        if(_eventTable.TryGetValue(eventType, out var del))
        {
            (del as Action<T1,T2>)?.Invoke(arg1, arg2);
        }
    }
    #endregion
    #region 3个参数事件
    // 添加监听
    public void AddListener<T1,T2,T3>(T eventType,Action<T1,T2,T3> listener)
    {
        if(listener == null)return;
        if(!_eventTable.ContainsKey(eventType))
        {
            _eventTable[eventType] = listener;
        }
        else
        {
            _eventTable[eventType] = Delegate.Combine(_eventTable[eventType], listener);
        }
    }

    // 移除监听
    public void RemoveListener<T1,T2,T3>(T eventType,Action<T1,T2,T3> listener)
    {
        if(listener == null)return;
        if(_eventTable.TryGetValue(eventType, out var del))
        {
            var newDel = Delegate.Remove(del, listener);
            if(newDel == null)
            {
                _eventTable.Remove(eventType);
            }
            else
            {
                _eventTable[eventType] = newDel;
            }
        }
    }

    // 触发监听
    public void Trigger<T1,T2,T3>(T eventType,T1 arg1,T2 arg2,T3 arg3)
    {
        if(_eventTable.TryGetValue(eventType, out var del))
        {
            (del as Action<T1,T2,T3>)?.Invoke(arg1, arg2, arg3);
        }
    }
    #endregion

     public void Clear()
    {
        _eventTable.Clear();
    }
}