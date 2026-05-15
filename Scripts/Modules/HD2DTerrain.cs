using Godot;
using System;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules
{
    public partial class HD2DTerrain : Node3D
    {
        [Export] public int Width = 20;
        [Export] public int Height = 20;
        [Export] public float TileSize = 1.0f;
        [Export] public float HeightScale = 1.0f;
        [Export] public Texture2D HeightmapTexture;

        private MeshInstance3D _terrainMesh;
        private CollisionShape3D _collisionShape;
        private Dictionary<Vector2, Vector3> _tilePositions = new Dictionary<Vector2, Vector3>();

        public override void _Ready()
        {
            Create3DTerrain();
            CreateCollision();
        }

        private void Create3DTerrain()
        {
            _terrainMesh = new MeshInstance3D();
            _terrainMesh.Name = "TerrainMesh";

            ArrayMesh mesh = new ArrayMesh();
            
            Vector3[] vertices = new Vector3[Width * Height];
            Vector2[] uv = new Vector2[Width * Height];
            int[] indices = new int[(Width - 1) * (Height - 1) * 6];

            int vertexIndex = 0;
            for (int z = 0; z < Height; z++)
            {
                for (int x = 0; x < Width; x++)
                {
                    float height = GetHeightAt(x, z);
                    vertices[vertexIndex] = new Vector3(
                        x * TileSize,
                        height * HeightScale,
                        z * TileSize
                    );
                    uv[vertexIndex] = new Vector2((float)x / Width, (float)z / Height);
                    _tilePositions[new Vector2(x, z)] = vertices[vertexIndex];
                    vertexIndex++;
                }
            }

            int indexIndex = 0;
            for (int z = 0; z < Height - 1; z++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    int v0 = z * Width + x;
                    int v1 = z * Width + x + 1;
                    int v2 = (z + 1) * Width + x;
                    int v3 = (z + 1) * Width + x + 1;

                    indices[indexIndex++] = v0;
                    indices[indexIndex++] = v2;
                    indices[indexIndex++] = v1;

                    indices[indexIndex++] = v1;
                    indices[indexIndex++] = v2;
                    indices[indexIndex++] = v3;
                }
            }

            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, new Godot.Collections.Array
            {
                vertices,
                uv,
                indices
            });

            _terrainMesh.Mesh = mesh;

            StandardMaterial3D material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.3f, 0.5f, 0.3f);
            material.SpecularMode = BaseMaterial3D.SpecularModeEnum.Disabled;
            material.Roughness = 1.0f;
            _terrainMesh.MaterialOverride = material;

            AddChild(_terrainMesh);
        }

        private float GetHeightAt(int x, int z)
        {
            if (HeightmapTexture != null)
            {
                Image image = HeightmapTexture.GetImage();
                int texX = Mathf.Clamp(x * image.GetWidth() / Width, 0, image.GetWidth() - 1);
                int texY = Mathf.Clamp(z * image.GetHeight() / Height, 0, image.GetHeight() - 1);
                Color pixel = image.GetPixel(texX, texY);
                return pixel.R;
            }
            
            return 0;
        }

        private void CreateCollision()
        {
            _collisionShape = new CollisionShape3D();
            _collisionShape.Name = "TerrainCollision";

            ConvexPolygonShape3D collisionShape = new ConvexPolygonShape3D();
            
            ArrayMesh mesh = _terrainMesh.Mesh as ArrayMesh;
            if (mesh != null)
            {
                Godot.Collections.Array arrays = mesh.SurfaceGetArrays(0);
                Vector3[] vertices = (Vector3[])arrays[0];
                collisionShape.SetPoints(vertices);
            }

            _collisionShape.Shape = collisionShape;
            AddChild(_collisionShape);
        }

        public Vector3 GetWorldPosition(Vector2 tilePosition)
        {
            if (_tilePositions.TryGetValue(tilePosition, out Vector3 position))
            {
                return new Vector3(
                    position.X + TileSize / 2,
                    position.Y,
                    position.Z + TileSize / 2
                );
            }
            return new Vector3(
                tilePosition.X * TileSize + TileSize / 2,
                0,
                tilePosition.Y * TileSize + TileSize / 2
            );
        }

        public float GetHeightAtWorldPosition(Vector3 worldPosition)
        {
            int x = Mathf.Clamp((int)(worldPosition.X / TileSize), 0, Width - 1);
            int z = Mathf.Clamp((int)(worldPosition.Z / TileSize), 0, Height - 1);
            
            if (_tilePositions.TryGetValue(new Vector2(x, z), out Vector3 pos))
            {
                return pos.Y;
            }
            return 0;
        }

        public bool IsValidTilePosition(Vector2 position)
        {
            return (int)position.X >= 0 && (int)position.X < Width &&
                   (int)position.Y >= 0 && (int)position.Y < Height;
        }
    }
}