﻿using System;
using KeatsLib.Persistence;
using UnityEngine;

public class SaveLoadManager : MonoSingleton<SaveLoadManager>, ISaveLoadManager
{
	public Persistence persistentData { get; private set; }
	[SerializeField] private bool mShouldSelfInitialize;

	private void Start()
	{
		if (!ServiceLocator.Get<IGamestateManager>().isAlive && mShouldSelfInitialize)
			LoadData();
	}
	
	public void LoadData()
	{
		Initialize();
	}

	private void Initialize()
	{
		//detect if this is a new save or not
		try
		{
			persistentData = Persistence.Load("/worldData.dat");
		}
		catch (Exception)
		{
			// ignored
		}
		
		// do some stuff async

		// then: 
		EventManager.Notify(EventManager.InitialPersistenceLoadComplete);
	}
}