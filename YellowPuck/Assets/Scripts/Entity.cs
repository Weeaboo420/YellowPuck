using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    void Die();
}

public class Entity : IEntity
{
    private string _name;
    private int _currentHealth, _maxHealth;
    private GameObject _parentObject;

    public Entity(string Name, int MaxHealth, GameObject ParentObject)
    {
        if (Name.Length > 0)
        {
            _name = Name;
        }
        else
        {
            Debug.LogError("Entity name length must be greater than 0!");
        }

        if (MaxHealth > 0)
        {
            _maxHealth = MaxHealth;
            _currentHealth = _maxHealth;
        }
        else
        {
            Debug.LogError("Entity max health must be greater than 0!");
        }

        _parentObject = ParentObject;
    }

    public string Name
    {
        get
        {
            return _name;
        }

        set
        {
            if(value.Length > 0)
            {
                _name = value;
            } else
            {
                Debug.LogError("Entity name length must be greater than 0!");
            }
        }
    }

    public int MaxHealth
    {
        get
        {
            return _maxHealth;
        }

        set
        {
            if(value > 0)
            {
                _maxHealth = value;
                if(_currentHealth > _maxHealth)
                {
                    _currentHealth = _maxHealth;
                }
            } else
            {
                Debug.LogError("Entity max health must be greater than 0!");
            }
        }
    }

    public int CurrentHealth
    {
        get
        {
            return _currentHealth;
        }

        set
        {
            if(value >= 0 && value <= _maxHealth)
            {
                _currentHealth = value;
            } else
            {
                Debug.LogError("Entity current health must be 0 or greater but not larger than the entity's max health!");
            }

            if(_currentHealth == 0)
            {
                Die();
            }

        }
    }

    public virtual void Die()
    {

    }

}

public class PlayerEntity : Entity
{
    private int _startingNodeIndex;    

    public PlayerEntity(string Name, int MaxHealth, GameObject ParentObject) : base(Name, MaxHealth, ParentObject) { }    

    public virtual void Die(Func<Action> DieFunction)
    {
        DieFunction();
    }

    public float Step
    {
        get
        {
            return Time.deltaTime * 4f;
        }
    }

    public int StartingNodeIndex
    {
        get
        {
            return _startingNodeIndex;
        }

        set
        {
            if (value >= 0)
            {
                _startingNodeIndex = value;
            }
        }
    }

}

public class GhostEntity : Entity
{

    private int _startingNodeIndex;

    public GhostEntity(string Name, int MaxHealth, GameObject ParentObject) : base(Name, MaxHealth, ParentObject) { }

    public float Step
    {
        get
        {
            return Time.deltaTime * 0.8f;
        }
    }

    public int StartingNodeIndex
    {
        get
        {
            return _startingNodeIndex;
        }

        set
        {
            if (value >= 0)
            {
                _startingNodeIndex = value;
            }
        }
    }

}