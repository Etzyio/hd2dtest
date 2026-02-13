using System.Security.Cryptography.X509Certificates;
using Godot;
using hd2dtest.Scripts.Core;
using static Godot.BaseMaterial3D;

public partial class HouseAreaHandler : CsgCombiner3D
{
	public Area3D HouseArea3D { get; set;}
	public CsgPolygon3D HouseNode { get; set; }
	public CsgBox3D box { get; set; }
	public CsgBox3D door { get; set; }

	public override void _Ready()
	{
		HouseNode = GetNode<CsgPolygon3D>("House");
		HouseArea3D = GetNode<Area3D>("House_Area3D");
		box = GetNode<CsgBox3D>("box");
		door = GetNode<CsgBox3D>("door");
		HouseArea3D.BodyEntered += OnHouseArea3DBodyEntered;
		HouseArea3D.BodyExited += OnHouseArea3DBodyExited;
	}

	public void OnHouseArea3DBodyEntered(Node body)
	{
		// 检查进入的是否是玩家角色
		if (body.Name == "Player" || body.IsInGroup("player"))
		{
			StandardMaterial3D material = (StandardMaterial3D)HouseNode.Material;
			material.CullMode = CullModeEnum.Front;
		}
	}

	public void OnHouseArea3DBodyExited(Node3D body)
	{
		// 检查离开的是否是玩家角色
		if (body.Name == "Player" || body.IsInGroup("player"))
		{
			StandardMaterial3D material = (StandardMaterial3D)HouseNode.Material;
			material.CullMode = CullModeEnum.Back;
		}
	}
}
