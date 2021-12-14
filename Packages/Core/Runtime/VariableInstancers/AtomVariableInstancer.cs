using Bolt;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityAtoms
{
    /// <summary>
    /// A Variable Instancer is a MonoBehaviour that takes a variable as a base and creates an in memory copy of it OnEnable.
    /// This is handy when you want to use atoms for prefabs that are instantiated at runtime. Use together with AtomCollection to
    /// react accordingly when a prefab with an associated atom is added or deleted to the scene.
    /// </summary>
    /// <typeparam name="V">Variable of type T.</typeparam>
    /// <typeparam name="P">IPair of type `T`.</typeparam>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="E1">Event of type T.</typeparam>
    /// <typeparam name="E2">Event x 2 of type T.</typeparam>
    /// <typeparam name="F">Function of type T => T</typeparam>

    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    [EditorIcon("atom-icon-hotpink")]
    [DefaultExecutionOrder(Runtime.ExecutionOrder.VARIABLE_INSTANCER)]
    public abstract class AtomVariableInstancer<V, P, T, E1, E2, F> : AtomBaseVariableInstancer<T, V>, IGetEvent, ISetEvent, IGetOrCreateEvent
        where V : AtomVariable<T, P, E1, E2, F>
        where P : struct, IPair<T>
        where E1 : AtomEvent<T>
        where E2 : AtomEvent<P>
        where F : AtomFunction<T, T>
    {
        [SerializeField] protected bool _overwriteInitialValue;

        [SerializeField] protected T _newInitialValue;

        /// <summary>
        /// Override to add implementation specific setup on `OnEnable`.
        /// </summary>
        protected override void ImplSpecificSetup()
        {
            if (_overwriteInitialValue)
            {
                _inMemoryCopy.InitialValue = _newInitialValue;
                _inMemoryCopy.SetValue(_newInitialValue);
            }

            if (Base.Changed != null)
            {
                _inMemoryCopy.Changed = Instantiate(Base.Changed);
            }

            if (Base.ChangedWithHistory != null)
            {
                _inMemoryCopy.ChangedWithHistory = Instantiate(Base.ChangedWithHistory);
            }

            // Manually trigger initial events since base class has already instantiated Variable
            // and the Variable's OnEnable hook has therefore already been executed.

            if (_overwriteInitialValue)
                Debug.Log("OVERWRITE");
            _inMemoryCopy.TriggerInitialEvents();
        }

        /// <summary>
        /// Get event by type.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <returns>The event.</returns>
        public E GetEvent<E>() where E : AtomEventBase
        {
            return _inMemoryCopy.GetEvent<E>();
        }

        /// <summary>
        /// Set event by type.
        /// </summary>
        /// <param name="e">The new event value.</param>
        /// <typeparam name="E"></typeparam>
        public void SetEvent<E>(E e) where E : AtomEventBase
        {
            _inMemoryCopy.SetEvent<E>(e);
        }

        /// <summary>
        /// Get event by type. Creates it if it doesn't exist.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <returns>The event.</returns>
        public E GetOrCreateEvent<E>() where E : AtomEventBase
        {
            return _inMemoryCopy.GetOrCreateEvent<E>();
        }

#if UNITY_EDITOR

        private static Vector2 offset = new Vector2(20, 0);
        private static Color baseMissingColor = new Color(0.98f, 0.78f, 1f);
        private static Color overwriteValueColor = new Color(0.39f, 0.52f, 0.44f);

        static AtomVariableInstancer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            // Get current game object as a variable instancer
            GameObject gameObj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            AtomVariableInstancer<V, P, T, E1, E2, F> instancer = gameObj?.GetComponent<AtomVariableInstancer<V, P, T, E1, E2, F>>();
            Rect offsetRect = new Rect(selectionRect.position + offset, selectionRect.size);


            if (instancer is { })
            {
                if (instancer._base == null)
                {
                    EditorGUI.DrawRect(selectionRect, baseMissingColor);
                    EditorGUI.LabelField(offsetRect, gameObj.name, new GUIStyle()
                        {
                            normal = new GUIStyleState() {textColor = Color.black},
                            fontStyle = FontStyle.Normal
                        }
                    );
                }
                else if (instancer._overwriteInitialValue)
                {
                    EditorGUI.DrawRect(selectionRect, overwriteValueColor);
                    EditorGUI.LabelField(offsetRect, gameObj.name, new GUIStyle()
                        {
                            normal = new GUIStyleState() {textColor = Color.white},
                            fontStyle = FontStyle.Normal
                        }
                    );
                }
            }


        }
#endif
    }
}

namespace Bolt
{
}
