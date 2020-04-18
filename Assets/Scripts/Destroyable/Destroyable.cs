using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace DestroyableObject
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Destroyable))]
	public class Destroyable_Editor : DestroyableEditor<Destroyable>
	{
		private bool showReplace;

		private bool showEvents;

		protected override void OnInspector()
		{
			var updateMesh = false;

			DrawDefault("Indestructible");
			DrawDefault("RecordAlphaCount");
			DrawDefault("AutoSharpen");

			Separator();

			DrawDefault("AutoSplit");

			if (Any(t => t.AutoSplit == true))
			{
				BeginIndent();
					DrawDefault("SplitExpand");
					BeginError(Any(t => t.SplitPixelsMin < 0));
						DrawDefault("SplitPixelsMin");
					EndError();
					BeginError(Any(t => t.SplitFeather < 0));
						DrawDefault("SplitFeather");
					EndError();
				EndIndent();
			}

			Separator();

			DrawDefault("MainTex");
			DrawDefault("DensityTex");
			DrawDefault("HealTex");
			DrawDefault("Sharpness");
			DrawDefault("color", ref updateMesh);

			DrawDamage();

			Separator();

			BeginDisabled(true);
			{
				BeginMixed(Any(t => t.AlphaTex != Target.AlphaTex));
					EditorGUI.ObjectField(DestroyableHelper.Reserve(), "Alpha Tex", Target.AlphaTex, typeof(Texture2D), false);
				EndMixed();
				BeginMixed(Any(t => t.AlphaCount != Target.AlphaCount));
					EditorGUI.IntField(DestroyableHelper.Reserve(), "Alpha Count", Target.AlphaCount);
				EndMixed();

				if (Targets.Length == 1 && Target.RecordAlphaCount == true)
				{
					EditorGUI.IntField(DestroyableHelper.Reserve(), "Original Alpha Count", Target.OriginalAlphaCount);
					EditorGUI.ProgressBar(DestroyableHelper.Reserve(), Target.RemainingAlpha, "Remaining Alpha");
				}
			}
			EndDisabled();

			Separator();

			showReplace = EditorGUI.Foldout(DestroyableHelper.Reserve(), showReplace, "Replace");

			if (showReplace == true)
			{
				DrawReplaceWith();
				DrawReplaceTextureWith();
				DrawReplaceAlphaWith();
			}

			Separator();

			showEvents = EditorGUI.Foldout(DestroyableHelper.Reserve(), showEvents, "Show Events");

			if (showEvents == true)
			{
				DrawDefault("OnStartSplit");
				DrawDefault("OnEndSplit");
				DrawDefault("OnDamageChanged");
				DrawDefault("OnAlphaDataReplaced");
				DrawDefault("OnAlphaDataModified");
				DrawDefault("OnAlphaDataSubset");
			}

			if (updateMesh == true) DirtyEach(t => t.UpdateMesh());
		}

		private void DrawDamage()
		{
			BeginMixed(Any(t => t.Damage != Target.Damage));
				var newDamage = EditorGUI.FloatField(DestroyableHelper.Reserve(), "Damage", Target.Damage);

				if (newDamage != Target.Damage)
				{
					Each(t => { t.Damage = newDamage; DestroyableHelper.SetDirty(t); });
				}
			EndMixed();
		}

		private void DrawReplaceWith()
		{
			var rect  = DestroyableHelper.Reserve();
			var right = rect; right.xMin += EditorGUIUtility.labelWidth;
			var rect1 = right; rect1.xMax -= rect1.width / 2;
			var rect2 = right; rect2.xMin += rect2.width / 2;

			EditorGUI.LabelField(rect, "With");

			var replaceSprite = (Sprite)EditorGUI.ObjectField(rect1, "", default(Object), typeof(Sprite), true);

			if (replaceSprite != null)
			{
				Each(t => { t.ReplaceWith(replaceSprite); DestroyableHelper.SetDirty(t); });
			}

			var replaceTexture2D = (Texture2D)EditorGUI.ObjectField(rect2, "", default(Object), typeof(Texture2D), true);

			if (replaceTexture2D != null)
			{
				Each(t => { t.ReplaceWith(replaceTexture2D); DestroyableHelper.SetDirty(t); });
			}
		}

		private void DrawReplaceTextureWith()
		{
			var rect  = DestroyableHelper.Reserve();
			var right = rect; right.xMin += EditorGUIUtility.labelWidth;
			var rect1 = right; rect1.xMax -= rect1.width / 2;
			var rect2 = right; rect2.xMin += rect2.width / 2;

			EditorGUI.LabelField(rect, "Texture With");

			var replaceSprite = (Sprite)EditorGUI.ObjectField(rect1, "", default(Object), typeof(Sprite), true);

			if (replaceSprite != null)
			{
				Each(t => { t.ReplaceTextureWith(replaceSprite); DestroyableHelper.SetDirty(t); });
			}

			var replaceTexture2D = (Texture2D)EditorGUI.ObjectField(rect2, "", default(Object), typeof(Texture2D), true);

			if (replaceTexture2D != null)
			{
				Each(t => { t.ReplaceTextureWith(replaceTexture2D); DestroyableHelper.SetDirty(t); });
			}
		}

		private void DrawReplaceAlphaWith()
		{
			var rect  = DestroyableHelper.Reserve();
			var right = rect; right.xMin += EditorGUIUtility.labelWidth;
			var rect1 = right; rect1.xMax -= rect1.width / 2;
			var rect2 = right; rect2.xMin += rect2.width / 2;

			EditorGUI.LabelField(rect, "Alpha With");

			var replaceSprite = (Sprite)EditorGUI.ObjectField(rect1, "", default(Object), typeof(Sprite), true);

			if (replaceSprite != null)
			{
				Each(t => { t.ReplaceAlphaWith(replaceSprite); DestroyableHelper.SetDirty(t); });
			}

			var replaceTexture2D = (Texture2D)EditorGUI.ObjectField(rect2, "", default(Object), typeof(Texture2D), true);

			if (replaceTexture2D != null)
			{
				Each(t => { t.ReplaceAlphaWith(replaceTexture2D); DestroyableHelper.SetDirty(t); });
			}
		}
	}
}
#endif

namespace DestroyableObject
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public partial class Destroyable : MonoBehaviour
	{
		public static List<Destroyable> AllDestructibles = new List<Destroyable>();
		private static int[] indices = new int[] { 0, 2, 1, 3, 1, 2 };
		private static Vector3[] positions = new Vector3[4];
		private static Vector2[] coords = new Vector2[4];
		private static Color[] colors = new Color[4];
		private static MaterialPropertyBlock propertyBlock;
		private static List<Destroyable> clones = new List<Destroyable>();

		public DestroyableEvent OnAlphaDataReplaced;
		public DestroyableRectEvent OnAlphaDataModified;
		public DestroyableRectEvent OnAlphaDataSubset;
		public DestroyableFloatFloatEvent OnDamageChanged;
		public DestroyableEvent OnStartSplit;
		public DestroyableListEvent OnEndSplit;

		[System.NonSerialized] public bool IsSplitting;

		// Is OnStartSplit currently being invoked? (used to prevent collider generation issues)
		[System.NonSerialized] public bool IsOnStartSplit;

		public Texture MainTex;
		public Texture2D DensityTex;
		public Texture2D HealTex;
		public bool Indestructible;
		public bool RecordAlphaCount = true;
		public bool AutoSharpen = true;
		public bool AutoSplit;
		public int SplitPixelsMin = 5;
		public int SplitExpand = -1;
		public int SplitFeather = 1;

		[Range(1.0f, 5.0f)] public float Sharpness = 1.0f;

		[SerializeField] private Color color = Color.white;
		[SerializeField] private float damage;
		[SerializeField] private Rect textureRect;
		[SerializeField] private Rect originalRect;
		[SerializeField] private Rect alphaRect;
		[SerializeField] private int alphaWidth;
		[SerializeField] private int alphaHeight;
		[SerializeField] private int alphaCount = -1;
		[SerializeField] private int originalAlphaCount = -1;
		[SerializeField] private byte[] alphaData;
		[System.NonSerialized] private Texture2D alphaTex;
		[System.NonSerialized] private DestroyableRect alphaDirty;
		[System.NonSerialized] private DestroyableRect alphaModified;
		[SerializeField] private Vector2 alphaScale;
		[SerializeField] private Vector2 alphaOffset;
		[System.NonSerialized] private float alphaRatio;
		[System.NonSerialized] private Mesh mesh;
		[System.NonSerialized] private MeshRenderer meshRenderer;
		[System.NonSerialized] private MeshFilter meshFilter;



		#region Properties
		public Color Color
		{
			set
			{
				if (color != value)
				{
					color = value;

					if (mesh != null)
					{
						UpdateMeshColors();
					}
				}
			}

			get
			{
				return color;
			}
		}

		public float Damage
		{
			set
			{
				if (damage != value && Indestructible == false)
				{
					var old = damage;

					damage = value;

					if (OnDamageChanged != null) OnDamageChanged.Invoke(old, value);
				}
			}

			get
			{
				return damage;
			}
		}

		public Texture2D AlphaTex
		{
			get
			{
				DeserializeAlphaTex();

				return alphaTex;
			}
		}

		public int AlphaWidth
		{
			get
			{
				return alphaWidth;
			}
		}

		public int AlphaHeight
		{
			get
			{
				return alphaHeight;
			}
		}

		public int AlphaCount
		{
			get
			{
				if (alphaCount == -1)
				{
					alphaCount = 0;

					var total = alphaWidth * alphaHeight;

					for (var i = 0; i < total; i++)
					{
						if (alphaData[i] > 127)
						{
							alphaCount += 1;
						}
					}
				}

				return alphaCount;
			}
		}

		public int OriginalAlphaCount
		{
			set
			{
				originalAlphaCount = value;
			}

			get
			{
				// Recalculate? This shouldn't be done here really
				if (originalAlphaCount == -1)
				{
					originalAlphaCount = AlphaCount;
				}

				return originalAlphaCount;
			}
		}

		public float RemainingAlpha
		{
			get
			{
				return DestroyableHelper.Divide(AlphaCount, OriginalAlphaCount);
			}
		}

		public byte[] AlphaData
		{
			get
			{
				return alphaData;
			}
		}

		public bool AlphaIsValid
		{
			get
			{
				return DestroyableHelper.AlphaIsValid(alphaData, alphaWidth, alphaHeight);
			}
		}

		public Rect AlphaRect
		{
			get
			{
				return alphaRect;
			}
		}

		public Rect TextureRect
		{
			get
			{
				return textureRect;
			}
		}

		public Rect OriginalRect
		{
			get
			{
				return originalRect;
			}
		}

		public Matrix4x4 AlphaToWorldMatrix
		{
			get
			{
				var matrix1 = DestroyableHelper.ScalingMatrix(alphaRect.width, alphaRect.height, 1.0f);
				var matrix2 = DestroyableHelper.TranslationMatrix(alphaRect.x, alphaRect.y, 0.0f);
				
				return transform.localToWorldMatrix * matrix2 * matrix1;
			}
		}

		public Matrix4x4 WorldToAlphaMatrix
		{
			get
			{
				/*
				var invX    = D2dHelper.Reciprocal(alphaRect.width);
				var invY    = D2dHelper.Reciprocal(alphaRect.height);
				var matrix1 = D2dHelper.ScalingMatrix(invX, invY, 1.0f);
				var matrix2 = D2dHelper.TranslationMatrix(-alphaRect.x, -alphaRect.y, 0.0f);
				
				return matrix1 * matrix2 * transform.worldToLocalMatrix;
				*/
				var matrix1 = Matrix4x4.identity;
				var matrix2 = Matrix4x4.identity;

				matrix1.m00 = DestroyableHelper.Reciprocal(alphaRect.width);
				matrix1.m11 = DestroyableHelper.Reciprocal(alphaRect.height);

				matrix2.m03 = -alphaRect.x;
				matrix2.m13 = -alphaRect.y;

				return matrix1 * matrix2 * transform.worldToLocalMatrix;
			}
		}
		#endregion




		public void SetIndestructible(bool newIndestructible)
		{
			Indestructible = newIndestructible;
		}

		public void Clear()
		{
			ClearAlpha();
		}

		public void ClearAlpha()
		{
			alphaData   = null;
			alphaWidth  = 0;
			alphaHeight = 0;
			alphaCount  = 0;
			alphaTex    = DestroyableHelper.Destroy(alphaTex);
		}

		public float SampleAlpha(Vector3 worldPosition)
		{
			var uv = (Vector2)WorldToAlphaMatrix.MultiplyPoint(worldPosition);

			if (DestroyableHelper.IsValidUV(uv) == true)
			{
				var x = Mathf.FloorToInt(uv.x * alphaWidth );
				var y = Mathf.FloorToInt(uv.y * alphaHeight);

				return SampleAlpha(x + y * alphaWidth);
			}

			return 0.0f;
		}

		public static DestroyableHit RaycastAlphaFirst(Vector3 startPosition, Vector3 endPosition)
		{
			var bestDistance = float.PositiveInfinity;
			var bestHit = default(DestroyableHit);

			for (var i = AllDestructibles.Count - 1; i >= 0; i--)
			{
				var destructible = AllDestructibles[i];
				var hit     = destructible.RaycastAlpha(startPosition, endPosition);

				if (hit != null && hit.Distance < bestDistance)
				{
					bestDistance = hit.Distance;
					bestHit = hit;
				}
			}

			return bestHit;
		}

		public DestroyableHit RaycastAlpha(Vector3 positionStart, Vector3 positionEnd)
		{
			var pointStart = WorldToAlphaMatrix.MultiplyPoint(positionStart);
			var pointEnd   = WorldToAlphaMatrix.MultiplyPoint(positionEnd  );
			var pixelStart = default(DestroyableVector2Operator);
			var pixelEnd   = default(DestroyableVector2Operator);

			pixelStart.X = Mathf.RoundToInt(pointStart.x * alphaWidth );
			pixelStart.Y = Mathf.RoundToInt(pointStart.y * alphaHeight);
			pixelEnd.X   = Mathf.RoundToInt(pointEnd.x * alphaWidth );
			pixelEnd.Y   = Mathf.RoundToInt(pointEnd.y * alphaHeight);

			return RaycastAlpha(pixelStart, pixelEnd);
		}

		public DestroyableHit RaycastAlpha(DestroyableVector2Operator start, DestroyableVector2Operator end)
		{
			var bounds    = default(Bounds);
			var distance  = default(float);
			var direction = (end - start).V.normalized;

			bounds.SetMinMax(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(alphaWidth, alphaHeight, 1.0f));

			if (start != end && bounds.IntersectRay(new Ray(start.V, direction), out distance) == true)
			{
				if (distance > 0)
				{
					start.X = Mathf.RoundToInt(start.X + direction.x * distance);
					start.Y = Mathf.RoundToInt(start.Y + direction.y * distance);
				}

				if (start != end && bounds.IntersectRay(new Ray(end.V, -direction), out distance) == true)
				{
					if (distance > 0)
					{
						end.X = Mathf.RoundToInt(end.X - direction.x * distance);
						end.Y = Mathf.RoundToInt(end.Y - direction.y * distance);
					}

					var width  = end.X - start.X;
					var height = end.Y - start.Y;
					var dx1    = 0;
					var dy1    = 0;
					var dx2    = 0;
					var dy2    = 0;

					if (width  < 0) dx1 = -1; else if (width  > 0) dx1 = 1;
					if (height < 0) dy1 = -1; else if (height > 0) dy1 = 1;
					if (width  < 0) dx2 = -1; else if (width  > 0) dx2 = 1;

					var longest  = Mathf.Abs(width);
					var shortest = Mathf.Abs(height);

					if (longest <= shortest)
					{
						longest  = Mathf.Abs(height);
						shortest = Mathf.Abs(width);
						dx2 = 0;

						if (height < 0) dy2 = -1; else if (height > 0) dy2 = 1;
					}

					var x    = start.X;
					var y    = start.Y;
					var numerator = longest >> 1;

					for (int i = 0; i <= longest; i++)
					{
						if (SampleAlpha(x, y) >= 0.5f)
						{
							var hit = new DestroyableHit();

							hit.Pixel.X  = x;
							hit.Pixel.Y  = y;
							hit.Point.x  = (float)x / (float)alphaWidth;
							hit.Point.y  = (float)y / (float)alphaHeight;
							hit.Position = AlphaToWorldMatrix.MultiplyPoint(hit.Point);
							hit.Distance = Vector2.Distance(new Vector2(x, y), new Vector2(start.X, start.Y));

							return hit;
						}

						numerator += shortest;

						if (!(numerator < longest))
						{
							numerator -= longest;
							x    += dx1;
							y    += dy1;
						}
						else
						{
							x += dx2;
							y += dy2;
						}
					}
				}
			}

			return null;
		}

		public static Matrix4x4 CalculateSliceMatrix(Vector2 startPos, Vector2 endPos, float thickness)
		{
			var mid   = (startPos + endPos) / 2.0f;
			var vec   = endPos - startPos;
			var size  = new Vector2(thickness, vec.magnitude);
			var angle = DestroyableHelper.Atan2(vec) * -Mathf.Rad2Deg;

			return CalculateStampMatrix(mid, size, angle);
		}

		public static Matrix4x4 CalculateStampMatrix(Vector2 position, Vector2 size, float angle)
		{
			var t = DestroyableHelper.TranslationMatrix(position.x, position.y, 0.0f);
			var r = DestroyableHelper.RotationMatrix(Quaternion.Euler(0.0f, 0.0f, angle));
			var s = DestroyableHelper.ScalingMatrix(size.x, size.y, 1.0f);
			var o = DestroyableHelper.TranslationMatrix(-0.5f, -0.5f, 0.0f); // Centre stamp

			return t * r * s * o;
		}

		public static void SliceAll(Vector2 startPos, Vector2 endPos, float thickness, Texture2D stampTex, float hardness, int layerMask = -1)
		{
			StampAll(CalculateSliceMatrix(startPos, endPos, thickness), stampTex, hardness, layerMask);
		}

		public static void StampAll(Vector2 position, Vector2 size, float angle, Texture2D stampTex, float hardness, int layerMask = -1)
		{
			StampAll(CalculateStampMatrix(position, size, angle), stampTex, hardness, layerMask);
		}

		public static void StampAll(Matrix4x4 matrix, Texture2D stampTex, float hardness, int layerMask = -1)
		{
			for (var i = AllDestructibles.Count - 1; i >= 0; i--)
			{
				var destructible = AllDestructibles[i];

				if (destructible != null && destructible.Indestructible == false)
				{
					var mask = 1 << destructible.gameObject.layer;

					if ((layerMask & mask) != 0)
					{
						destructible.BeginAlphaModifications();
						{
							destructible.Stamp(matrix, stampTex, hardness);
						}
						destructible.EndAlphaModifications();
					}
				}
			}
		}

		public void Slice(Vector2 startPos, Vector2 endPos, float thickness, Texture2D stampTex, float hardness)
		{
			Stamp(CalculateSliceMatrix(startPos, endPos, thickness), stampTex, hardness);
		}

		public void Stamp(Vector2 position, Vector2 size, float angle, Texture2D stampTex, float hardness)
		{
			Stamp(CalculateStampMatrix(position, size, angle), stampTex, hardness);
		}
		
		public DestroyableRect OriginalRectRelative
		{
			get
			{
				var originalW = DestroyableHelper.Divide(originalRect.width , alphaRect.width ) * alphaWidth;
				var originalH = DestroyableHelper.Divide(originalRect.height, alphaRect.height) * alphaHeight;
				var x    = Mathf.CeilToInt((originalRect.x - alphaRect.x) * originalW);
				var y    = Mathf.CeilToInt((originalRect.y - alphaRect.y) * originalH);
				var w    = Mathf.FloorToInt(originalW);
				var h    = Mathf.FloorToInt(originalH);
				
				return DestroyableRect.CreateFromMinSize(x, y, w, h);
			}
		}

		public void AddDamage(int amount)
		{
			Damage += amount;
		}

		public void RemoveDamage(int amount)
		{
			Damage -= amount;
		}

		public void Stamp(Matrix4x4 stampMatrix, Texture2D stampTex, float hardness)
		{
			if (AlphaIsValid == false)
			{
				throw new System.InvalidOperationException("Invalid alpha");
			}

			if (stampTex == null)
			{
				return;
			}

			if (hardness == 0.0f)
			{
				return;
			}

			if (hardness < 0.0f && HealTex == null)
			{
				return; // Does healing without a HealTex make sense?
			}
#if UNITY_EDITOR
			DestroyableHelper.MakeTextureReadable(stampTex);
			DestroyableHelper.MakeTextureReadable(DensityTex);
			DestroyableHelper.MakeTextureReadable(HealTex);
#endif
			var matrix = WorldToAlphaMatrix * stampMatrix;
			var rect   = default(DestroyableRect);

			if (DestroyableHelper.CalculateRect(matrix, ref rect, alphaWidth, alphaHeight) == true)
			{
				
				rect.MinX = Mathf.Clamp(rect.MinX, 0, alphaWidth );
				rect.MaxX = Mathf.Clamp(rect.MaxX, 0, alphaWidth );
				rect.MinY = Mathf.Clamp(rect.MinY, 0, alphaHeight);
				rect.MaxY = Mathf.Clamp(rect.MaxY, 0, alphaHeight);

				var inverse    = matrix.inverse;
				var alphaPixelX     = DestroyableHelper.Reciprocal(alphaWidth);
				var alphaPixelY     = DestroyableHelper.Reciprocal(alphaHeight);
				var alphaHalfPixelX = alphaPixelX * 0.5f;
				var alphaHalfPixelY = alphaPixelY * 0.5f;

				// Heal?
				if (hardness < 0.0f)
				{
					if (HealTex != null)
					{
						var healOffsetX = (alphaRect.x - originalRect.x) / originalRect.width;
						var healOffsetY = (alphaRect.y - originalRect.y) / originalRect.height;
						var healScaleX  = alphaRect.width / originalRect.width;
						var healScaleY  = alphaRect.height / originalRect.height;

						for (var y = rect.MinY; y < rect.MaxY; y++)
						{
							var v = y * alphaPixelY + alphaHalfPixelY;

							for (var x = rect.MinX; x < rect.MaxX; x++)
							{
								var u  = x * alphaPixelX + alphaHalfPixelX;
								var stampUV = inverse.MultiplyPoint(new Vector3(u, v, 0.0f));

								if (DestroyableHelper.IsValidUV(stampUV) == true)
								{
									var healU = u * healScaleX + healOffsetX;
									var healV = v * healScaleY + healOffsetY;

									if (DestroyableHelper.IsValidUV(healU, healV) == true)
									{
										var index = x + y * alphaWidth;
										var alpha = SampleAlpha(index);
										var stamp = SampleStamp(stampTex, stampUV) * hardness;
										var heal  = SampleHeal(healU, healV);
										
										WriteAlpha(index, Mathf.Min(alpha - stamp, heal));
									}
								}
							}
						}
					}
					else
					{
						for (var y = rect.MinY; y < rect.MaxY; y++)
						{
							var v = y * alphaPixelY + alphaHalfPixelY;

							for (var x = rect.MinX; x < rect.MaxX; x++)
							{
								var u  = x * alphaPixelX + alphaHalfPixelX;
								var stampUV = inverse.MultiplyPoint(new Vector3(u, v, 0.0f));

								if (DestroyableHelper.IsValidUV(stampUV) == true)
								{
									var index = x + y * alphaWidth;
									var alpha = SampleAlpha(index);
									var stamp = SampleStamp(stampTex, stampUV) * hardness;

									WriteAlpha(index, alpha - stamp);
								}
							}
						}
					}
				}
				// Damaged
				else
				{
					if (DensityTex != null)
					{
						var densityOffsetX = (alphaRect.x - originalRect.x) / originalRect.width;
						var densityOffsetY = (alphaRect.y - originalRect.y) / originalRect.height;
						var densityScaleX  = alphaRect.width / originalRect.width;
						var densityScaleY  = alphaRect.height / originalRect.height;

						for (var y = rect.MinY; y < rect.MaxY; y++)
						{
							var v = y * alphaPixelY + alphaHalfPixelY;

							for (var x = rect.MinX; x < rect.MaxX; x++)
							{
								var u  = x * alphaPixelX + alphaHalfPixelX;
								var stampUV = inverse.MultiplyPoint(new Vector3(u, v, 0.0f));

								if (DestroyableHelper.IsValidUV(stampUV) == true)
								{
									var densityU = u * densityScaleX + densityOffsetX;
									var densityV = v * densityScaleY + densityOffsetY;

									if (DestroyableHelper.IsValidUV(densityU, densityV) == true)
									{
										var index   = x + y * alphaWidth;
										var alpha   = SampleAlpha(index);
										var stamp   = SampleStamp(stampTex, stampUV) * hardness;
										var density = SampleDensity(densityU, densityV);

										if (stamp > density)
										{
											WriteAlpha(index, alpha - (stamp - density));
										}
									}
								}
							}
						}
					}
					else
					{
						for (var y = rect.MinY; y < rect.MaxY; y++)
						{
							var v = y * alphaPixelY + alphaHalfPixelY;

							for (var x = rect.MinX; x < rect.MaxX; x++)
							{
								var u  = x * alphaPixelX + alphaHalfPixelX;
								var stampUV = inverse.MultiplyPoint(new Vector3(u, v, 0.0f));

								if (DestroyableHelper.IsValidUV(stampUV) == true)
								{
									var index = x + y * alphaWidth;
									var alpha = SampleAlpha(index);
									var stamp = SampleStamp(stampTex, stampUV) * hardness;

									WriteAlpha(index, alpha - stamp);
								}
							}
						}
					}
				}

				alphaModified.Add(rect);
			}
		}

		public void WriteAlpha(int x, int y, byte alpha)
		{
			if (x >= 0 && y >= 0 && x < alphaWidth && y < alphaHeight)
			{
				alphaModified.Add(x, y);

				alphaData[x + y * alphaWidth] = alpha;
			}
		}
		
		public void BeginAlphaModifications()
		{
			alphaModified.Clear();
		}

		public void EndAlphaModifications()
		{
			if (alphaModified.IsSet == true)
			{
				alphaDirty.Add(alphaModified);

				alphaCount = -1;

				if (AutoSplit == true)
				{
					var rect = alphaModified;

					if (SplitExpand < 0)
					{
						rect.Expand(Mathf.Max(alphaWidth, alphaHeight));
					}
					else
					{
						rect.Expand(SplitExpand);
					}

					if (TrySplit(rect) == false)
					{
						if (OnAlphaDataModified != null) OnAlphaDataModified.Invoke(alphaModified);
					}
				}
				else
				{
					if (OnAlphaDataModified != null) OnAlphaDataModified.Invoke(alphaModified);
				}
			}
		}

		public float SampleAlpha(int x, int y)
		{
			if (x >= 0 && y >= 0 && x < alphaWidth && y < alphaHeight)
			{
				return SampleAlpha(x + y * alphaWidth);
			}

			return 0.0f;
		}

		private float SampleAlpha(int i)
		{
			return DestroyableHelper.ConvertAlpha(alphaData[i]);
		}

		private void WriteAlpha(int i, float alpha)
		{
			alphaData[i] = DestroyableHelper.ConvertAlpha(Mathf.Clamp01(alpha));
		}

		private float SampleStamp(Texture2D texture2D, Vector2 uv)
		{
			var x = (int)(uv.x * texture2D.width);
			var y = (int)(uv.y * texture2D.height);

			return texture2D.GetPixel(x, y).a;
		}

		private float SampleDensity(float u, float v)
		{
			var x = (int)(u * DensityTex.width );
			var y = (int)(v * DensityTex.height);

			return DensityTex.GetPixel(x, y).a;
		}

		private float SampleHeal(float u, float v)
		{
			var x = (int)(u * HealTex.width );
			var y = (int)(v * HealTex.height);

			return HealTex.GetPixel(x, y).a;
		}

		public void ReplaceWith(Sprite sprite)
		{
			if (sprite != null)
			{
				var texture2D = sprite.texture;

				if (texture2D != null)
				{
					var scale = 1.0f / sprite.pixelsPerUnit;

					originalRect.x = (-sprite.pivot.x + Mathf.Ceil(sprite.textureRectOffset.x)) * scale;
					originalRect.y = (-sprite.pivot.y + Mathf.Ceil(sprite.textureRectOffset.y)) * scale;
					originalRect.width  = Mathf.Floor(sprite.textureRect.width) * scale;
					originalRect.height = Mathf.Floor(sprite.textureRect.height) * scale;

					MainTex = texture2D;

					alphaRect = originalRect;

					ReplaceAlphaWith(sprite);

					UpdateMesh();
				}
			}
		}

		public void ReplaceWith(Texture2D texture2D)
		{
			if (texture2D != null)
			{
				originalRect.x = texture2D.width * -0.5f;
				originalRect.y = texture2D.height * -0.5f;
				originalRect.width  = texture2D.width;
				originalRect.height = texture2D.height;

				MainTex = texture2D;

				alphaRect = originalRect;

				ReplaceAlphaWith(texture2D);

				UpdateMesh();
			}
		}

		public void ReplaceTextureWith(Texture2D texture2D)
		{
			if (texture2D != null)
			{
				MainTex = texture2D;
			}
		}

		public void ReplaceTextureWith(Sprite sprite)
		{
			if (sprite != null)
			{
				var texture2D = sprite.texture;

				if (texture2D != null)
				{
					var scale   = 1.0f / sprite.pixelsPerUnit;

					originalRect.x = (-sprite.pivot.x + Mathf.Ceil(sprite.textureRectOffset.x)) * scale;
					originalRect.y = (-sprite.pivot.y + Mathf.Ceil(sprite.textureRectOffset.y)) * scale;
					originalRect.width  = Mathf.Floor(sprite.textureRect.width) * scale;
					originalRect.height = Mathf.Floor(sprite.textureRect.height) * scale;

					MainTex = texture2D;

					var texRect   = sprite.textureRect;
					var texX = Mathf.CeilToInt(texRect.x);
					var texY = Mathf.CeilToInt(texRect.y);
					var texWidth  = Mathf.FloorToInt(texRect.width);
					var texHeight = Mathf.FloorToInt(texRect.height);

					textureRect.x = DestroyableHelper.Divide(texX, texture2D.width);
					textureRect.y = DestroyableHelper.Divide(texY, texture2D.height);
					textureRect.width  = DestroyableHelper.Divide(texWidth, texture2D.width);
					textureRect.height = DestroyableHelper.Divide(texHeight, texture2D.height);

					UpdateMesh();
				}
			}
		}

		public void ReplaceAlphaWith(DestroyableSnapshot snapshot)
		{
			if (snapshot != null)
			{
				alphaRect = snapshot.AlphaRect;

				ReplaceAlphaWith(snapshot.AlphaData, snapshot.AlphaWidth, snapshot.AlphaHeight);
			}
		}

		public void ReplaceAlphaWith(Sprite sprite)
		{
			if (sprite != null)
			{
				var texture2D = sprite.texture;

				if (texture2D != null)
				{
					var texRect   = sprite.textureRect;
					var texX = Mathf.CeilToInt(texRect.x);
					var texY = Mathf.CeilToInt(texRect.y);
					var texWidth  = Mathf.FloorToInt(texRect.width);
					var texHeight = Mathf.FloorToInt(texRect.height);

					ReplaceAlphaWith(texture2D, texX, texY, texWidth, texHeight);
				}
			}
		}

		public void ReplaceAlphaWith(Texture2D texture2D)
		{
			if (texture2D != null)
			{
				ReplaceAlphaWith(texture2D, 0, 0, texture2D.width, texture2D.height);
			}
		}

		public void ReplaceAlphaWith(Texture2D texture2D, int x, int y, int width, int height, int newAlphaCount = -1)
		{
			if (DestroyableHelper.ExtractAlpha(texture2D, x, y, width, height) == true)
			{
				textureRect.x = DestroyableHelper.Divide(x, texture2D.width);
				textureRect.y = DestroyableHelper.Divide(y, texture2D.height);
				textureRect.width  = DestroyableHelper.Divide(width, texture2D.width);
				textureRect.height = DestroyableHelper.Divide(height, texture2D.height);

				ReplaceAlphaWith(DestroyableHelper.AlphaData, width, height, newAlphaCount);
			}
		}

		public void ReplaceAlphaWith(byte[] newAlphaData, int newAlphaWidth, int newAlphaHeight, int newAlphaCount = -1)
		{
			if (newAlphaData != null && newAlphaWidth > 0 && newAlphaHeight > 0)
			{
				var newAlphaTotal = newAlphaWidth * newAlphaHeight;

				if (newAlphaData.Length >= newAlphaTotal)
				{
					FastCopyAlphaData(newAlphaData, newAlphaWidth, newAlphaHeight, newAlphaCount);

					if (RecordAlphaCount == true)
					{
						originalAlphaCount = AlphaCount;
					}
					else
					{
						originalAlphaCount = -1;
					}

					alphaDirty.Set(0, newAlphaWidth, 0, newAlphaHeight);

					if (OnAlphaDataReplaced != null) OnAlphaDataReplaced.Invoke();
				}
			}
		}

		public void UpdateAlpha(byte[] newAlphaData, DestroyableRect newAlphaRect)
		{
			if (AlphaIsValid == true)
			{
				if (DestroyableHelper.AlphaIsValid(newAlphaData, newAlphaRect) == true)
				{
					var oldRect = new DestroyableRect(0, alphaWidth, 0, alphaHeight);
					var lapRect = DestroyableRect.CalculateOverlap(oldRect, newAlphaRect);

					if (lapRect.IsSet == true)
					{
						for (var y = lapRect.MinY; y < lapRect.MaxY; y++)
						{
							//var oy = y * AlphaWidth;

							for (var x = lapRect.MinX; x < lapRect.MaxX; x++)
							{
								var nx = x - newAlphaRect.MinX;
								var ny = y - newAlphaRect.MinY;
								var na = newAlphaData[ny * newAlphaRect.SizeX + nx];

								WriteAlpha(x, y, na);
							}
						}

						alphaModified.Set(lapRect.MinX, lapRect.MaxX, lapRect.MinY, lapRect.MaxY);

						alphaDirty.Add(alphaModified);

						if (OnAlphaDataModified != null) OnAlphaDataModified.Invoke(alphaModified);
					}
				}
				else
				{
					Debug.LogError("invalid size " + newAlphaData.Length + " - " + newAlphaRect.SizeX + " - " + newAlphaRect.SizeY);
				}
			}
			else
			{
				Debug.LogError("Alpha INvalid");
			}
		}
		
		private void FastCopyAlphaData(byte[] newAlphaData, int newAlphaWidth, int newAlphaHeight, int newAlphaCount = -1)
		{
			var newAlphaTotal = newAlphaWidth * newAlphaHeight;

			if (alphaData == null || alphaData.Length != newAlphaTotal)
			{
				alphaData = new byte[newAlphaTotal];
			}

			for (var i = newAlphaTotal - 1; i >= 0; i--)
			{
				alphaData[i] = newAlphaData[i];
			}

			alphaWidth  = newAlphaWidth;
			alphaHeight = newAlphaHeight;
			alphaCount  = newAlphaCount;
			alphaRatio  = 0.0f;
		}

		public DestroyableSnapshot GetSnapshot(DestroyableSnapshot snapshot = null)
		{
			if (AlphaIsValid == true)
			{
				if (snapshot == null) snapshot = new DestroyableSnapshot();

				var alphaTotal = alphaWidth * alphaHeight;

				if (snapshot.AlphaData == null || snapshot.AlphaData.Length <= alphaTotal)
				{
					snapshot.AlphaData = new byte[alphaTotal];
				}

				for (var i = 0; i < alphaTotal; i++)
				{
					snapshot.AlphaData[i] = alphaData[i];
				}

				snapshot.AlphaWidth  = alphaWidth;
				snapshot.AlphaHeight = alphaHeight;
				snapshot.AlphaRect   = alphaRect;

				return snapshot;
			}

			return null;
		}

		[ContextMenu("Optimize Alpha")]
		public void OptimizeAlpha()
		{
			TrimAlpha();

			BlurAlpha(false);
			HalveAlpha(true);

			TrimAlpha();
		}

		[ContextMenu("Halve Alpha")]
		public void HalveAlpha()
		{
#if UNITY_EDITOR
			DestroyableHelper.SetDirty(this);
#endif
			HalveAlpha(true);
		}

		public void HalveAlpha(bool replace)
		{
			var oldWidth  = alphaWidth;
			var oldHeight = alphaHeight;

			DestroyableHelper.Halve(ref alphaData, ref alphaWidth, ref alphaHeight);
			
			var pixelW = DestroyableHelper.Reciprocal(oldWidth ) * alphaRect.width;
			var pixelH = DestroyableHelper.Reciprocal(oldHeight) * alphaRect.height;

			alphaRect.xMin += pixelW * 0.5f;
			alphaRect.xMax -= pixelW * 0.5f;
			alphaRect.yMin += pixelH * 0.5f;
			alphaRect.yMax -= pixelH * 0.5f;

			if (replace == true)
			{
				ReplaceAlphaWith(alphaData, alphaWidth, alphaHeight);
			}
		}

		[ContextMenu("Blur Alpha")]
		public void BlurAlpha()
		{
#if UNITY_EDITOR
			DestroyableHelper.SetDirty(this);
#endif
			BlurAlpha(true);
		}

		public void BlurAlpha(bool replace)
		{
			DestroyableHelper.Blur(alphaData, alphaWidth, alphaHeight);

			if (replace == true)
			{
				ReplaceAlphaWith(alphaData, alphaWidth, alphaHeight);
			}
		}

		[ContextMenu("Trim Alpha")]
		public void TrimAlpha()
		{
			if (AlphaIsValid == false)
			{
				throw new System.InvalidOperationException("Invalid alpha");
			}
#if UNITY_EDITOR
			DestroyableHelper.SetDirty(this);
#endif
			var xMin = 0;
			var xMax = alphaWidth;
			var yMin = 0;
			var yMax = alphaHeight;

			for (var x = xMin; x < xMax; x++)
			{
				if (FastSolidAlphaVertical(yMin, yMax, x) == false) xMin += 1; else break;
			}

			for (var x = xMax - 1; x >= xMin; x--)
			{
				if (FastSolidAlphaVertical(yMin, yMax, x) == false) xMax -= 1; else break;
			}

			for (var y = yMin; y < yMax; y++)
			{
				if (FastSolidAlphaHorizontal(xMin, xMax, y) == false) yMin += 1; else break;
			}

			for (var y = yMax - 1; y >= yMin; y--)
			{
				if (FastSolidAlphaHorizontal(xMin, xMax, y) == false) yMax -= 1; else break;
			}

			var width  = xMax - xMin + 2;
			var height = yMax - yMin + 2;
			var rect   = DestroyableRect.CreateFromMinSize(xMin - 1, yMin - 1, width, height);

			DestroyableHelper.ClearAlpha(width, height);

			DestroyableHelper.PasteAlpha(alphaData, alphaWidth, xMin, xMax, yMin, yMax, 1, 1, width);

			SubsetAlphaWith(DestroyableHelper.AlphaData, rect, alphaCount);
		}

		private bool FastSolidAlphaHorizontal(int xMin, int xMax, int y)
		{
			var offset = y * alphaWidth;

			for (var x = xMin; x < xMax; x++)
			{
				if (alphaData[x + offset] > 0)
				{
					return true;
				}
			}

			return false;
		}

		private bool FastSolidAlphaVertical(int yMin, int yMax, int x)
		{
			for (var y = yMin; y < yMax; y++)
			{
				if (alphaData[x + y * alphaWidth] > 0)
				{
					return true;
				}
			}

			return false;
		}

		[ContextMenu("Reset Alpha")]
		public void ResetAlpha()
		{
			var texture2D = MainTex as Texture2D;

			if (texture2D != null)
			{
				var x = Mathf.RoundToInt(textureRect.x * MainTex.width);
				var y = Mathf.RoundToInt(textureRect.y * MainTex.height);
				var w = Mathf.RoundToInt(textureRect.width  * MainTex.width);
				var h = Mathf.RoundToInt(textureRect.height * MainTex.height);

				alphaRect = originalRect;

				UpdateMesh();

				ReplaceAlphaWith(texture2D, x, y, w, h);
			}
		}

		//[ContextMenu("Try Split")]
		//public bool TrySplit()
		//{
		//	var rect = new DestroyableRect();

		//	rect.MaxX = alphaWidth;
		//	rect.MaxY = alphaHeight;

		//	return TrySplit(rect);
		//}

		private static DestroyableDistanceField distanceField = new DestroyableDistanceField();

		public bool TrySplit(DestroyableRect splitRect)
		{
			if (IsSplitting == true)
			{
				return false;
			}

			if (AlphaIsValid == false)
			{
				throw new System.InvalidOperationException("Invalid alpha");
			}

			splitRect.ClampTo(0, alphaWidth, 0, alphaHeight);

			if (splitRect.IsSet == false)
			{
				throw new System.ArgumentOutOfRangeException("Split rect does not overlap alpha");
			}

			DestroyableLogFloodfill.FastFind(alphaData, alphaWidth, alphaHeight, splitRect);

			for (var i = DestroyableLogFloodfill.IslandsA.Count - 1; i >= 0; i--)
			{
				var island = DestroyableLogFloodfill.IslandsA[i];

				if (island.Count < SplitPixelsMin)
				{
				}
			}

			DestroyableSplitGroup.ClearAll();


			switch (DestroyableLogFloodfill.IslandLayout)
			{
				case DestroyableLogFloodfill.Layout.Whole:
				{
					if (DestroyableLogFloodfill.IslandsA.Count > 1)
					{
						var baseRect = splitRect; baseRect.Expand(SplitFeather); baseRect.ClampTo(0, alphaWidth, 0, alphaHeight);

						distanceField.Transform(baseRect, alphaWidth, alphaHeight, alphaData);

						for (var i = DestroyableLogFloodfill.IslandsA.Count - 1; i >= 0; i--)
						{
							var island     = DestroyableLogFloodfill.IslandsA[i];
							var islandRect = new DestroyableRect(island.MinX, island.MaxX, island.MinY, island.MaxY); islandRect.Expand(SplitFeather); islandRect.ClampTo(splitRect);
							var splitGroup = DestroyableSplitGroup.GetSplitGroup();

							island.Submit(distanceField, splitGroup, baseRect, islandRect);
						}

						SplitWhole(DestroyableSplitGroup.SplitGroups);

						return true;
					}
				}
				break;

			}

			return false;
		}

		public void SplitWhole(List<DestroyableSplitGroup> groups)
		{
			if (groups == null || groups.Count < 2)
			{
				return;
			}

			IsSplitting = true;
			clones.Clear();
			{
				IsOnStartSplit = true;
				{
					if (OnStartSplit != null) OnStartSplit.Invoke();
				}
				IsOnStartSplit = false;

				var originalData   = alphaData;
				var originalWidth  = alphaWidth;
				var originalHeight = alphaHeight;

				// Null data so it doesn't get cloned
				alphaData = null;

				for (var i = groups.Count - 1; i >= 0; i--)
				{
					var group = groups[i];
					var clone = i == 0 ? this : CloneForSplit();

					group.GenerateData();

					group.CombineData(originalData, originalWidth, originalHeight);

					clone.SubsetAlphaWith(group.Data, group.Rect);

					clones.Add(clone);
				}

				if (OnEndSplit != null) OnEndSplit.Invoke(clones);
			}
			IsSplitting = false;
			clones.Clear();
		}

		private Destroyable CloneForSplit()
		{
			var clone = Instantiate(this);

			clone.name = name;
			clone.tag  = tag;

			clone.gameObject.layer = gameObject.layer;

			clone.transform.SetParent(transform.parent, false);

			clone.transform.localPosition = transform.localPosition;
			clone.transform.localRotation = transform.localRotation;
			clone.transform.localScale    = transform.localScale;

			return clone;
		}

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (meshFilter == null) meshFilter = gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

			if (MainTex != null)
			{
				var paddedTextureRect = textureRect;

				FastPadRect(ref paddedTextureRect);

				if (mesh == null)
				{
					mesh = new Mesh();

					UpdateMeshData(paddedTextureRect);

					mesh.hideFlags = HideFlags.DontSave;
					mesh.name = "Destructible Mesh";
					mesh.triangles = indices;
				}
				else
				{
					UpdateMeshData(paddedTextureRect);
				}

				UpdateProperties(paddedTextureRect);

				meshFilter.sharedMesh = mesh;
			}
			else
			{
				meshFilter.sharedMesh = null;
			}
		}

		public void SubsetAlphaWith(byte[] subData, DestroyableRect subRect, int newAlphaCount = -1)
		{
			if (DestroyableHelper.AlphaIsValid(subData, subRect) == false)
			{
				throw new System.ArgumentException("Invalid subset data");
			}
			
			var stepX = DestroyableHelper.Divide(alphaRect.width , alphaWidth );
			var stepY = DestroyableHelper.Divide(alphaRect.height, alphaHeight);

			alphaRect.x += stepX * subRect.MinX;
			alphaRect.y += stepY * subRect.MinY;
			alphaRect.width  += stepX * (subRect.SizeX - alphaWidth );
			alphaRect.height += stepY * (subRect.SizeY - alphaHeight);

			FastCopyAlphaData(subData, subRect.SizeX, subRect.SizeY, newAlphaCount);

			UpdateMesh();

			alphaDirty.Set(0, subRect.SizeX, 0, subRect.SizeY);

			if (OnAlphaDataSubset != null) OnAlphaDataSubset.Invoke(subRect);
		}

		protected virtual void Awake()
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();

			if (spriteRenderer != null)
			{
				var sprite    = spriteRenderer.sprite;
				var sortingOrder   = spriteRenderer.sortingOrder;
				var sortingLayerID = spriteRenderer.sortingLayerID;
				var material  = spriteRenderer.sharedMaterial;

				DestroyImmediate(spriteRenderer);

				ReplaceWith(sprite);

				UpdateMesh();
				UpdateRenderer(material);

				var sorter = gameObject.AddComponent<DestroyableSorter>();

				sorter.SortingOrder   = sortingOrder;
				sorter.SortingLayerID = sortingLayerID;
			}
			else
			{
				UpdateMesh();
				UpdateRenderer(null);
			}
		}

		protected virtual void OnEnable()
		{
			AllDestructibles.Add(this);
		}

		protected virtual void OnDisable()
		{
			AllDestructibles.Remove(this);
		}

		protected virtual void OnWillRenderObject()
		{
			DeserializeAlphaTex();

			if (MainTex != null && alphaTex != null)
			{
				if (meshRenderer  == null) meshRenderer  = gameObject.GetComponent<MeshRenderer>();
				if (meshRenderer  == null) meshRenderer  = gameObject.AddComponent<MeshRenderer>();
				if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

				var alphaSharpness = Sharpness;

				if (AutoSharpen == true)
				{
					if (alphaRatio <= 0.0f)
					{
						var textureWidth = DestroyableHelper.Divide(alphaRect.width, originalRect.width) * MainTex.width * textureRect.width;

						alphaRatio = DestroyableHelper.Divide(textureWidth, alphaWidth);
					}

					alphaSharpness *= alphaRatio;
				}

				propertyBlock.SetTexture("_MainTex", MainTex);
				propertyBlock.SetTexture("_AlphaTex", alphaTex);
				propertyBlock.SetVector("_AlphaScale", alphaScale);
				propertyBlock.SetVector("_AlphaOffset", alphaOffset);
				propertyBlock.SetFloat("_AlphaSharpness", alphaSharpness);

				meshRenderer.SetPropertyBlock(propertyBlock);
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireCube(alphaRect.center, alphaRect.size);

			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

			Gizmos.DrawWireCube(originalRect.center, originalRect.size);
		}
#endif
		private void UpdateRenderer(Material oldMaterial)
		{
			if (meshRenderer == null) meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

			if (meshRenderer.sharedMaterial == null)
			{
				//paso de ponerlo en resources.
				//meshRenderer.sharedMaterial = Resources.Load<Material>("SpriteDestruible");
			}
		}
		
		private void FastPadRect(ref Rect rect)
		{
			if (SystemInfo.npotSupport == NPOTSupport.Full)
			{
				return;
			}

			if (SystemInfo.npotSupport == NPOTSupport.Restricted)
			{
				var texture2D = MainTex as Texture2D;

				if (texture2D != null && texture2D.mipmapCount <= 1)
				{
					return;
				}
			}

			var width     = MainTex.width;
			var height    = MainTex.height;
			var padWidth  = Mathf.NextPowerOfTwo(width );
			var padHeight = Mathf.NextPowerOfTwo(height);

			if (width != padWidth)
			{
				var scale = width / (float)padWidth;

				rect.x     *= scale;
				rect.width *= scale;
			}

			if (height != padHeight)
			{
				var scale = height / (float)padHeight;

				rect.y *= scale;
				rect.height *= scale;
			}
		}

		private static readonly Vector3[] quadNormals = new Vector3[] { new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, -1.0f) };

		private static readonly Vector4[] quadTangents = new Vector4[] { new Vector4(1.0f, 0.0f, 0.0f, -1.0f), new Vector4(1.0f, 0.0f, 0.0f, -1.0f), new Vector4(1.0f, 0.0f, 0.0f, -1.0f), new Vector4(1.0f, 0.0f, 0.0f, -1.0f) };

		private void UpdateMeshData(Rect paddedTextureRect)
		{
			positions[0] = new Vector3(alphaRect.xMin, alphaRect.yMin, 0.0f);
			positions[1] = new Vector3(alphaRect.xMax, alphaRect.yMin, 0.0f);
			positions[2] = new Vector3(alphaRect.xMin, alphaRect.yMax, 0.0f);
			positions[3] = new Vector3(alphaRect.xMax, alphaRect.yMax, 0.0f);

			mesh.vertices = positions;
			
			UpdateMeshColors();
			
			UpdateMeshCoords(paddedTextureRect);
			
			mesh.normals  = quadNormals;
			mesh.tangents = quadTangents;
			
			mesh.RecalculateBounds();
		}
		
		private void UpdateMeshColors()
		{
			colors[0] = Color;
			colors[1] = Color;
			colors[2] = Color;
			colors[3] = Color;

			mesh.colors = colors;
		}

		private void UpdateMeshCoords(Rect paddedTextureRect)
		{
			var l = DestroyableHelper.InverseLerp(originalRect.xMin, originalRect.xMax, alphaRect.xMin);
			var r = DestroyableHelper.InverseLerp(originalRect.xMin, originalRect.xMax, alphaRect.xMax);
			var b = DestroyableHelper.InverseLerp(originalRect.yMin, originalRect.yMax, alphaRect.yMin);
			var t = DestroyableHelper.InverseLerp(originalRect.yMin, originalRect.yMax, alphaRect.yMax);

			var xMin = paddedTextureRect.x + paddedTextureRect.width  * l;
			var xMax = paddedTextureRect.x + paddedTextureRect.width  * r;
			var yMin = paddedTextureRect.y + paddedTextureRect.height * b;
			var yMax = paddedTextureRect.y + paddedTextureRect.height * t;

			coords[0] = new Vector2(xMin, yMin);
			coords[1] = new Vector2(xMax, yMin);
			coords[2] = new Vector2(xMin, yMax);
			coords[3] = new Vector2(xMax, yMax);

			mesh.uv = coords;
		}

		private void UpdateProperties(Rect paddedTextureRect)
		{
			alphaScale.x = DestroyableHelper.Divide(DestroyableHelper.Divide(originalRect.width , alphaRect.width ), paddedTextureRect.width );
			alphaScale.y = DestroyableHelper.Divide(DestroyableHelper.Divide(originalRect.height, alphaRect.height), paddedTextureRect.height);

			alphaOffset.x = paddedTextureRect.x + paddedTextureRect.width  * DestroyableHelper.Divide(alphaRect.x - originalRect.x, originalRect.width );
			alphaOffset.y = paddedTextureRect.y + paddedTextureRect.height * DestroyableHelper.Divide(alphaRect.y - originalRect.y, originalRect.height);
		}

		private void DeserializeAlphaTex()
		{
			if (AlphaIsValid == true)
			{
				if (alphaTex == null)
				{
					ConstructAlphaTex();
				}
				else if (alphaTex.width != alphaWidth || alphaTex.height != alphaHeight)
				{
					alphaTex = DestroyableHelper.Destroy(alphaTex);

					ConstructAlphaTex();
				}
				else if (alphaDirty.IsSet == true)
				{
					ReconstructAlphaTex();
				}
			}
			else
			{
				Clear();
			}
		}

		private void ConstructAlphaTex()
		{
			alphaTex = new Texture2D(alphaWidth, alphaHeight, TextureFormat.Alpha8, false);

			alphaTex.hideFlags = HideFlags.DontSave;
			alphaTex.wrapMode  = TextureWrapMode.Clamp;

			for (var y = 0; y < alphaHeight; y++)
			{
				for (var x = 0; x < alphaWidth; x++)
				{
					var color = default(Color);
					var alpha = alphaData[x + y * alphaWidth];

					color.a = DestroyableHelper.ConvertAlpha(alpha);

					alphaTex.SetPixel(x, y, color);
				}
			}

			alphaTex.Apply();

			alphaDirty.Clear();
		}

		private void ReconstructAlphaTex()
		{
			var xMin = alphaDirty.MinX;
			var xMax = alphaDirty.MaxX;
			var yMin = alphaDirty.MinY;
			var yMax = alphaDirty.MaxY;

			for (var y = yMin; y < yMax; y++)
			{
				for (var x = xMin; x < xMax; x++)
				{
					var color = default(Color);
					var alpha = alphaData[x + y * alphaWidth];

					color.a = DestroyableHelper.ConvertAlpha(alpha);

					alphaTex.SetPixel(x, y, color);
				}
			}

			alphaTex.Apply();

			alphaDirty.Clear();
		}
	}
}
