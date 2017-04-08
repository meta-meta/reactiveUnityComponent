/*
* JACK4U
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JACK4U{

	public class JACK4USpectrumilizer : MonoBehaviour {
		
		public delegate void SampleEventHandler(Object sender,float[] sampleDataLeft,float[] sampleDataRight);
	    public event SampleEventHandler SampleEvent;
		
		public GameObject prefab;
		public int sampleRate= 1024;//64,128,256,512,1024,2048,4096,8192
		public bool visualize;
		public int spectrumItems= 32;
		public Vector3 positionOffset;
		public float scale= 1f;
		[Range(2,10)]
		public int logScale= 2;
		public float yOffset;
		
		private float[] sampleDataLeft;
		private float[] sampleDataRight;
		private List<GameObject> spectrumGOListLeft;
		private List<GameObject> spectrumGOListRight;
		private AudioListener listener;
		private Vector3 position;
		private Vector3 positionR;
		
		private float dataItemsPerSpectrumItem;
		private float[] spectrumDataLeft;
		private float[] spectrumDataRight;

		private float[] sampleDataLeftSum;
		private float[] sampleDataRightSum;

		private void RaiseSampleEvent()
		{
			if(SampleEvent != null) SampleEvent(this, spectrumDataLeft,spectrumDataRight);
		}

		void Start () {

		}
		void OnEnable(){

			sampleDataLeft = new float[sampleRate];
			sampleDataRight = new float[sampleRate];
		
			spectrumGOListLeft = new List<GameObject>();
			spectrumGOListRight = new List<GameObject>();
			GameObject currGO_L;
			GameObject currGO_R;
			position = Vector3.zero ;
			positionR = position + (Vector3.up* yOffset) ;
			
			dataItemsPerSpectrumItem = sampleRate / (float)spectrumItems;
			spectrumDataLeft = new float[spectrumItems];
			spectrumDataRight = new float[spectrumItems];

			if(visualize){	
				for(int i = 0;i< spectrumItems;i++){
					currGO_L = ((GameObject)Instantiate(prefab,position,Quaternion.identity));
					spectrumGOListLeft.Add (currGO_L);
					currGO_L.transform.parent = this.transform;
					currGO_L.transform.localPosition = position;
					position+= positionOffset;

					currGO_R = ((GameObject)Instantiate(prefab,position,Quaternion.identity));
					spectrumGOListRight.Add (currGO_R);
					currGO_R.transform.parent = this.transform;
					currGO_R.transform.localPosition = positionR;
					positionR+= positionOffset;
				}
			}

		}

		void OnDisable(){
			foreach(var go in spectrumGOListLeft){
				Destroy(go);
			}
			foreach(var go in spectrumGOListRight){
				Destroy(go);
			}
		}
		

		void Update () {
			AudioListener.GetSpectrumData(sampleDataLeft, 0, FFTWindow.BlackmanHarris);
			AudioListener.GetSpectrumData(sampleDataRight, 1, FFTWindow.BlackmanHarris);

			int indexGO;

			sampleDataLeftSum = new float[spectrumItems];
			sampleDataRightSum = new float[spectrumItems];

			for (int i = 0; i < sampleRate; i++) {
				indexGO = (int)(i/dataItemsPerSpectrumItem);
				sampleDataLeftSum[indexGO]+= sampleDataLeft[i];
				sampleDataRightSum[indexGO]+= sampleDataRight[i];	


			}

			
			for( int i = 0;i< spectrumItems;i++){
				spectrumDataLeft[i] = sampleDataLeftSum[i]/ (float)dataItemsPerSpectrumItem;
				spectrumDataRight[i] = sampleDataRightSum[i]/ (float)dataItemsPerSpectrumItem;
				//Debug.Log("i:"+i+"::"+Mathf.Log(i));
				if (visualize )spectrumGOListLeft[i].transform.localScale =  new Vector3(0.1f,Mathf.Log(i+1,logScale)*scale*(spectrumDataLeft[i]),0.1f);
				if (visualize )spectrumGOListRight[i].transform.localScale =  new Vector3(0.1f,Mathf.Log(i+1,logScale)*scale*(spectrumDataRight[i]),0.1f);
			}
			RaiseSampleEvent();

#if UNITY_EDITOR
			for (int i =0; i< spectrumItems-1;i++){
				Debug.DrawLine(new Vector3(Mathf.Log(i+1)*2.5f,scale* spectrumDataLeft[i], 0), new Vector3(Mathf.Log(i+2)*2.5f, scale*spectrumDataLeft[i+1] , 0), Color.red);
			}
#endif

			
		}
	}
}

