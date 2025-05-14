using Godot;
using System;

public interface ILiftable
{
	public enum Weight
	{
		Light = 0,
		Medium = 1,
		Heavy = 2
	}

	public Weight LiftWeight { get; set; }
}
