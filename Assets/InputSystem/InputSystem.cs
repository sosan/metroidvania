// GENERATED AUTOMATICALLY FROM 'Assets/InputSystem/InputAction.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace NewMovementSystem
{
    public class @InputSystem : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @InputSystem()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputAction"",
    ""maps"": [
        {
            ""name"": ""MapaMenu"",
            ""id"": ""47d13b7d-31c1-46f7-afbd-68f12335f999"",
            ""actions"": [
                {
                    ""name"": ""New action"",
                    ""type"": ""Button"",
                    ""id"": ""7387d46e-9833-4f1c-b878-74c11927cfa6"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""240f9bc8-5070-4ed5-89c2-8b280cc326dc"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""New action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""MapaPlayer"",
            ""id"": ""38ef7c74-8ec1-4974-9b3c-ea1ffd1ad79d"",
            ""actions"": [
                {
                    ""name"": ""PlayerMovement"",
                    ""type"": ""Button"",
                    ""id"": ""80a54f50-ec04-43e4-adee-a0ec8724c00d"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""LeftStick"",
                    ""id"": ""7a795812-e19d-41a6-bfce-ab9f6b3c04f9"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerMovement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""175c4b44-82c5-4c57-93a8-6e8bffc53354"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""2c6c4153-16ff-4273-ba3b-708291b62425"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""81ee982a-b208-4a22-8aeb-e9bc0f116d16"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""da9109ff-dd38-4f4c-9ade-0adee453250b"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""d506bb0b-7470-4268-b136-4e87ab2bb9ce"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4d11a1ac-8566-4547-a545-b667bfac8f3b"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a8235d43-698e-473f-8915-fe0c43b2220e"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""44c04afe-c3c6-4fbc-86b5-195ffa1eb998"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // MapaMenu
            m_MapaMenu = asset.FindActionMap("MapaMenu", throwIfNotFound: true);
            m_MapaMenu_Newaction = m_MapaMenu.FindAction("New action", throwIfNotFound: true);
            // MapaPlayer
            m_MapaPlayer = asset.FindActionMap("MapaPlayer", throwIfNotFound: true);
            m_MapaPlayer_PlayerMovement = m_MapaPlayer.FindAction("PlayerMovement", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        // MapaMenu
        private readonly InputActionMap m_MapaMenu;
        private IMapaMenuActions m_MapaMenuActionsCallbackInterface;
        private readonly InputAction m_MapaMenu_Newaction;
        public struct MapaMenuActions
        {
            private @InputSystem m_Wrapper;
            public MapaMenuActions(@InputSystem wrapper) { m_Wrapper = wrapper; }
            public InputAction @Newaction => m_Wrapper.m_MapaMenu_Newaction;
            public InputActionMap Get() { return m_Wrapper.m_MapaMenu; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(MapaMenuActions set) { return set.Get(); }
            public void SetCallbacks(IMapaMenuActions instance)
            {
                if (m_Wrapper.m_MapaMenuActionsCallbackInterface != null)
                {
                    @Newaction.started -= m_Wrapper.m_MapaMenuActionsCallbackInterface.OnNewaction;
                    @Newaction.performed -= m_Wrapper.m_MapaMenuActionsCallbackInterface.OnNewaction;
                    @Newaction.canceled -= m_Wrapper.m_MapaMenuActionsCallbackInterface.OnNewaction;
                }
                m_Wrapper.m_MapaMenuActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Newaction.started += instance.OnNewaction;
                    @Newaction.performed += instance.OnNewaction;
                    @Newaction.canceled += instance.OnNewaction;
                }
            }
        }
        public MapaMenuActions @MapaMenu => new MapaMenuActions(this);

        // MapaPlayer
        private readonly InputActionMap m_MapaPlayer;
        private IMapaPlayerActions m_MapaPlayerActionsCallbackInterface;
        private readonly InputAction m_MapaPlayer_PlayerMovement;
        public struct MapaPlayerActions
        {
            private @InputSystem m_Wrapper;
            public MapaPlayerActions(@InputSystem wrapper) { m_Wrapper = wrapper; }
            public InputAction @PlayerMovement => m_Wrapper.m_MapaPlayer_PlayerMovement;
            public InputActionMap Get() { return m_Wrapper.m_MapaPlayer; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(MapaPlayerActions set) { return set.Get(); }
            public void SetCallbacks(IMapaPlayerActions instance)
            {
                if (m_Wrapper.m_MapaPlayerActionsCallbackInterface != null)
                {
                    @PlayerMovement.started -= m_Wrapper.m_MapaPlayerActionsCallbackInterface.OnPlayerMovement;
                    @PlayerMovement.performed -= m_Wrapper.m_MapaPlayerActionsCallbackInterface.OnPlayerMovement;
                    @PlayerMovement.canceled -= m_Wrapper.m_MapaPlayerActionsCallbackInterface.OnPlayerMovement;
                }
                m_Wrapper.m_MapaPlayerActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @PlayerMovement.started += instance.OnPlayerMovement;
                    @PlayerMovement.performed += instance.OnPlayerMovement;
                    @PlayerMovement.canceled += instance.OnPlayerMovement;
                }
            }
        }
        public MapaPlayerActions @MapaPlayer => new MapaPlayerActions(this);
        public interface IMapaMenuActions
        {
            void OnNewaction(InputAction.CallbackContext context);
        }
        public interface IMapaPlayerActions
        {
            void OnPlayerMovement(InputAction.CallbackContext context);
        }
    }
}
