using Sirenix.OdinInspector;
using UnityEngine;

namespace Utils
{
	public class Layers
	{
		public const string DEFAULT = "Default";
		public const string CHARACTER = "Character";
		public const string RAGDOLL = "Ragdoll";
		public const string WEAPON = "Weapons";
		public const string OBJECTS = "Objects";
		public const string ENVIRONMENT = "Environment";
		public const string ENVIRONMENT_IGNORE_RAYCAST = "EnvironmentIgnoreRayCasts";
		public const string HEADS = "Heads";
		public const string CREATURES = "Creatures";
		public const string RAYCASTONLY = "OnlyRaycast";
		
		private static readonly Layer _defaultLayer = new Layer(DEFAULT);
		private static readonly Layer _characterLayer = new Layer(CHARACTER);
		private static readonly Layer _ragdollLayer = new Layer(RAGDOLL);
		private static readonly Layer _weaponLayer = new Layer(WEAPON);
		private static readonly Layer _headsLayer = new Layer(HEADS);
		private static readonly Layer _creaturesLayer = new Layer(CREATURES);
		private static readonly Layer _raycastOnly = new Layer(RAYCASTONLY);
		private static readonly Layer _environment = new Layer(ENVIRONMENT);

		public static int DefaultLayer => _defaultLayer.Id;
		public static int CharacterLayer => _characterLayer.Id;
		public static int RagdollLayer => _ragdollLayer.Id;
		public static int WeaponLayer => _weaponLayer.Id;
		public static int RayCastOnlyLayer => _raycastOnly.Id;
		public static int EnvironmentLayer => _environment.Id;

		private class Layer
		{
			private readonly string _name;

			private int? _id;

			public int Id
			{
				get
				{
					_id ??= LayerMask.NameToLayer(_name);
					return _id.Value;
				}
			}

			public Layer(string name)
			{
				_name = name;
			}
		}
		
		public static bool ContainsLayer(LayerMask layerMask, int layer)
		{
			return (1 << layer & layerMask) != 0;
		}

		public static bool ContainsLayer(int layerMask, int layer)
		{
			return (1 << layer & layerMask) != 0;
		}
	}

	public class LayerMasks
	{
		private static readonly Mask _hitObjectMask = new Mask(Layers.CREATURES, Layers.WEAPON, Layers.RAGDOLL,Layers.OBJECTS, Layers.ENVIRONMENT, Layers.HEADS,Layers.RAYCASTONLY);
		private static readonly Mask _interactionMask = new Mask(Layers.OBJECTS, Layers.RAGDOLL, Layers.WEAPON, Layers.ENVIRONMENT, Layers.HEADS, Layers.RAYCASTONLY, Layers.CREATURES);
		private static readonly Mask _walkableMask = new Mask(Layers.ENVIRONMENT, Layers.OBJECTS, Layers.HEADS, Layers.ENVIRONMENT_IGNORE_RAYCAST);
		private static readonly Mask _RagdollsObjects = new Mask(Layers.RAGDOLL, Layers.OBJECTS, Layers.HEADS, Layers.RAYCASTONLY, Layers.CREATURES);
		private static readonly Mask _enviroment = new Mask(Layers.ENVIRONMENT);
		private static readonly Mask _organic = new Mask(Layers.RAGDOLL, Layers.HEADS, Layers.CREATURES);
		private static readonly Mask _shooters = new Mask(Layers.CHARACTER);
		private static readonly Mask _heads = new Mask(Layers.HEADS);
		private static readonly Mask _creatures = new Mask(Layers.CREATURES);
		private static readonly Mask _c4Layers = new Mask(Layers.RAGDOLL,Layers.HEADS,Layers.ENVIRONMENT,Layers.OBJECTS);
		private static readonly Mask _laserMineLayers = new Mask(Layers.RAGDOLL,Layers.HEADS, Layers.CHARACTER, Layers.OBJECTS);
		private static readonly Mask _characterLayers = new Mask(Layers.RAGDOLL, Layers.CHARACTER);
		private static readonly Mask _allEntities = new Mask(Layers.CREATURES, Layers.CHARACTER, Layers.HEADS);

		public static ref int HitObjectMask => ref _hitObjectMask.Value;
		public static int WalkableMask => _walkableMask.Value;
		public static int Heads => _heads.Value;
		public static int Creatures => _creatures.Value;
		public static int InteractionMask => _interactionMask.Value;
		public static int Environment => _enviroment.Value;
		public static int RagdollObject => _RagdollsObjects.Value;
		public static int Shooters => _shooters.Value;
		public static int C4Layers => _c4Layers.Value;
		public static int LaserMineLayers => _laserMineLayers.Value;
		public static int CharacterLayers => _characterLayers.Value;
		public static int OrganicLayers => _organic.Value;
		public static int AllEntities => _allEntities.Value;

		public static ValueDropdownList<int> GetLayersMasks()
		{
			return new ValueDropdownList<int>()
			{
				{
					"HitObjectMask", HitObjectMask
				},
				{
					"WalkableMask", WalkableMask
				},
				{
					"InteractionMask", InteractionMask
				},
				{
					"Environment", Environment
				},
				{
					"RagdollObject", RagdollObject
				},
				{
					"Shooters", Shooters
				},
				{
					"Heads", Heads
				},
				{
					"Creatures", Creatures
				},
				{
					"OrganicLayers", OrganicLayers
				},
				{
					"AllEntities", AllEntities
				},
			};
		}
		

		
		private class Mask
		{
			private readonly string[] _layerNames;
			private bool _hasValue;
			private int _value;

			public Mask(params string[] layerNames)
			{
				_layerNames = layerNames;
			}

			public ref int Value
			{
				get
				{
					if (_hasValue)
						return ref _value;
					_value = LayerMask.GetMask(_layerNames);
					_hasValue = true;
					return ref _value;
				}
			}
		}
	}
}