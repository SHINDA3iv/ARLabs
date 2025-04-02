














using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Lab4
{
    public class CreatedObject : MonoBehaviour
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _description;
        private int _number = -1;

        // Добавляем переменные для сохранения исходной позиции и масштаба
        public Vector3 InitialLocalScale { get; private set; }

        public string Name
        {
            get
            {
                if (_number >= 0)
                {
                    return _displayName + " " + _number.ToString();
                }
                else
                {
                    return _displayName;
                }
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public void GiveNumber(int number)
        {
            _number = number;
        }

        public void SaveInitialState()
        {
            InitialLocalScale = transform.localScale;
            Debug.Log("[OBJECT_SELECTION_MODE] " + InitialLocalScale);
        }

        public void ResetToInitialState()
        {
            transform.localScale = InitialLocalScale;
            Debug.Log("[OBJECT_SELECTION_MODE] " + transform.localScale);
        }
    }
}


