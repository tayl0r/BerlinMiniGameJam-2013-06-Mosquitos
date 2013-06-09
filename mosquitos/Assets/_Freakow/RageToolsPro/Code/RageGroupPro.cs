using System;
using System.Collections.Generic;
using UnityEngine;

public partial class RageGroup {

	[SerializeField] private List<RageMagnet> _magnetList;
	public List<RageMagnet> MagnetList {
		get {
			if (_magnetList != null){
                _magnetList.RemoveAll(item => item == null);
                return _magnetList;			    
			}

			_magnetList = new List<RageMagnet>();
			return _magnetList;
		}
		set { _magnetList = value; }
	}

	[SerializeField] RageGroupMagnetData _magnetData;
	public RageGroupMagnetData MagnetData {
		get { return _magnetData ?? (_magnetData = new RageGroupMagnetData()); }
	    set { _magnetData = value; }
	}

	public void MagnetsForceRefresh() {       
		foreach (RageMagnet magnet in MagnetList)
			magnet.ForceRefresh = true;
	}

	public void MagnetsUpdateRestPositon() {
        foreach(RageMagnet magnet in MagnetList) 
            magnet.UpdateRestPosition();	
	}

}

[Serializable]
public class RageGroupMagnetData {
	public bool Live;
	public bool ShowGizmos;
	public bool DraftMode;
	public bool Normalize;
}
