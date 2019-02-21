using System;
using PseudoTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Priority_Queue;

public class NevPointMap : MonoBehaviour{
	[Header("NevPoints")]
	public BoxCollision boxCollision;
	public LayerMask solidLayer;
	public float step = 0.5f;
	private Vector2 originPos = Vector2.zero;
	public Rect edge = new Rect(Vector2.zero, new Vector2(3, 3));
	public static Vector2[] dirs = null;
	public static Vector2[] Dirs {
		get {
			if(dirs == null) {
				dirs = new Vector2[8];
				int dirIndex = 0;
				for(int i = 0; i < 3; i++) {
					for(int j = 0; j < 3; j++) {
						Vector2 dir = new Vector2(i - 1, j - 1);
						if(dir != Vector2.zero) {
							dirs[dirIndex] = dir;
							dirIndex++;
						}
					}
				}
				//dirs = new Vector2[]{Vector2.right, Vector2.left};
			}
			return dirs;
		}
	}
	[HideInInspector]
	[SerializeField]
	public TileMapInt indexTileMap;
	public NevPoint GetClosestNevPoint(Vector2 pos) {
		int index = indexTileMap[pos];
		if(index == -1) return null;
		return points[index];
	}
	[HideInInspector]
	public List<NevPoint> points;
	[Header("Gizmos")]
	public float gizmosCubeSize = 0.2f;
	public bool drawGizmosLine = false;
	public bool drawGizmos = false;
	private Vector2[] testPath;
	[Button("Bake")]
	public void Bake() {
		originPos = VectorUtils.V32(transform.position);
		indexTileMap = new TileMapInt(edge, originPos, step, -1);
		points.Clear();
		LinkedList<Vector2> queue = new LinkedList<Vector2>();
		queue.AddLast(originPos);
		Func<Vector2, NevPoint> insertNewPos = (pos) => {
			var nevPoint = new NevPoint(pos);
			indexTileMap[pos] = points.Count;
			points.Add(nevPoint);
			return nevPoint;
		};
		insertNewPos(originPos);
		while(queue.Count != 0) {
			Vector2 pos = queue.First.Value;
			queue.RemoveFirst();
			var nevPoint = GetClosestNevPoint(pos);
			foreach(var dir in Dirs) {
				Vector2 tar = pos + dir * step;
				if(!Physics2D.Raycast(pos, dir, step, solidLayer) && edge.Contains(tar) && (boxCollision == null || !boxCollision.CheckStaticCollision(solidLayer, tar))) {
					nevPoint.SetAroundFlag(dir, true);
					if(indexTileMap[tar] == -1) {
						insertNewPos(tar);
						queue.AddLast(tar);
					}
				}
			}
		}
	}
	public void OnDrawGizmosSelected() {
		if(!drawGizmos) {
			return;
		}
		Gizmos.color = Color.green;
		Gizmos.DrawCube(VectorUtils.V23(originPos), new Vector3(1,1,1) * gizmosCubeSize);
		Gizmos.DrawWireCube(edge.center, edge.size);
		if(points == null) return;
		Gizmos.color = Color.yellow;
		Action<Vector2> drawPoint = (pos) => Gizmos.DrawCube(VectorUtils.V23(pos), new Vector3(1,1,1) * gizmosCubeSize);
		Action<Vector2, Vector2> drawDir = (pos, dir) => Gizmos.DrawLine(pos, pos + dir * step);
		foreach(var point in points) {
			drawPoint(point.pos);
			if(drawGizmosLine) {
				foreach(var dir in Dirs) {
					if(point.GetAroundFlag(dir)) {
						drawDir(point.pos, dir);
					}
				}
			}
		}
		Gizmos.color = Color.blue;
		if(testPath != null) {
			for(int i = 0; i < testPath.Length - 1; i++) {
				Gizmos.DrawLine(testPath[i], testPath[i + 1]);
			}
		}

	}
	public static int GetDirIndex(Vector2 dir) {
		for(int i = 0; i < Dirs.Length; i++) {
			if(Dirs[i] == dir) {
				return i;
			}
		}
		return -1;
	}
	public Path CreatePath(Func<Vector2> posGetter, Vector2 target) {
		Debug.Assert(edge.Contains(posGetter()) && edge.Contains(target));
		Vector2[] smoothPath = findSmoothPath(posGetter(), target);
		Vector2[] removeFrom = new Vector2[smoothPath.Length - 1];
		for(int i = 0; i < removeFrom.Length; i++) {
			removeFrom[i] = smoothPath[i + 1];
		}
		Path path = new Path(posGetter(), removeFrom);
		StartCoroutine(pathUpdate(path, posGetter));
		return path;		
	}
	private IEnumerator pathUpdate(Path path, Func<Vector2> posGetter) {
		while(!path.Finished) {
			path.Update(posGetter());
			yield return new WaitForFixedUpdate();
		}
	}
	[Button("TestSmoothPath")]
	public void TestSmoothPath() {
		Func<Vector2> getProperRandomPos = () => {
			Vector2 pos = Vector2.zero;
			NevPoint center = null;
			Action a = () => {
				pos = getRandomPosInEdge();
				center = points[indexTileMap.GetClosestTile(pos, -1)];
			};
			a();
			while(!validSmoothPath(pos, center.pos)) {
				a();
			}
			return pos;
		};
		testPath = findSmoothPath(getProperRandomPos(), getProperRandomPos());
	}
	private bool validSmoothPath(Vector2 f, Vector2 t) {
		if(boxCollision == null) {
			return Physics2D.Raycast(f, t - f, (t - f).magnitude, solidLayer).collider == null;
		}
		else {
			return boxCollision.CheckMoveCollision(t - f, solidLayer, f) == null;
		}
	}
	private Vector2[] findSmoothPath(Vector2 from, Vector2 to) {
		Func<Vector2, NevPoint> getClosestPoint = (pos) => {
			var index = indexTileMap.GetClosestTile(pos, -1);
			Debug.Assert(index != -1);
			return points[index];
		};
		Debug.Assert(edge.Contains(from) && edge.Contains(to));
		NevPoint fromPoint = getClosestPoint(from);
		NevPoint toPoint = getClosestPoint(to);
		NevPoint[] aStarResult = aStarSearch(fromPoint, toPoint);

		// smooth path
		int smoothPathSize = aStarResult.Length + 4;
		Vector2[] smoothPath = new Vector2[smoothPathSize];
		int i = 0;
		Action<Vector2> addOne = (p) => smoothPath[i++] = p;
		Action<IEnumerable<NevPoint>> addMultiNevPoints = (ps) => {
			foreach(var p in ps) {
				addOne(p.pos);
			}
		};
		addOne(from);
		addOne(fromPoint.pos);
		addMultiNevPoints(aStarResult);
		addOne(toPoint.pos);
		addOne(to);
		return highSmooth(smoothPath);

	}
	private Vector2[] highSmooth(Vector2[] origin) {
		bool[] skipBuffer = new bool[origin.Length];
		int skipCount = 0;
		int last = 0;
		while(last != origin.Length - 1) {
			for(int i = origin.Length - 1; i > last; i--) {
				if(validSmoothPath(origin[last], origin[i])) {
					for(int j = last + 1; j < i; j++) {
						skipBuffer[j] = true;
						skipCount++;
					}
					last = i - 1;
				}
			}
			last++;
		}
		return skipArray(origin, skipBuffer, skipCount);

	}
	private T[] skipArray<T>(T[] origin, bool[] skipBuffer, int skipCount) {

		T[] result = new T[origin.Length - skipCount];
		int resultI = 0;
		for(int j = 0; j < skipBuffer.Length; j++) {
			if(!skipBuffer[j]) {
				result[resultI] = origin[j];
				resultI++;
			}
		}
		return result;
	}
	private Vector2[] quickSmooth(Vector2[] origin) {
		bool[] skipBuffer = new bool[origin.Length];
		int last = 0;
		int skipCount = 0;
		for(int j = 1; j < origin.Length - 1; j++) {
			if(validSmoothPath(origin[last], origin[j + 1])) {
				skipBuffer[j] = true;
				skipCount++;
			}
			else {
				last = j;
			}
		}
		return skipArray(origin, skipBuffer, skipCount);
	}
	private Vector2 getRandomPosInEdge() {
		return VectorUtils.Do(VectorUtils.Do(Vector2.zero, (f)=>UnityEngine.Random.value), edge.min, edge.max, (r, min, max) => Mathf.Lerp(min, max, r));
	}
	[Button("TestAstarSearch")]
	public void TestAstarSearch() {
		Func<NevPoint> getRandomNevPoint = () => {
			Func<NevPoint> gR = () => GetClosestNevPoint(getRandomPosInEdge());
			var random = gR();
			while(random == null) random = gR();
			return random;
		};
		NevPoint testFrom = getRandomNevPoint();
		NevPoint testTo = getRandomNevPoint();
		var result = aStarSearch(testFrom, testTo);
		testPath = new Vector2[result.Length + 2];
		testPath[0] = testFrom.pos;
		testPath[result.Length + 1] = testTo.pos;
		for(int i = 0; i < result.Length; i++) {
			testPath[i + 1] = result[i].pos;
		}
		return;
	}
	private NevPoint[] aStarSearch(NevPoint from, NevPoint to) {
		SimplePriorityQueue<NevPoint> searchQueue = new SimplePriorityQueue<NevPoint>();
		Dictionary<NevPoint, NevPoint> last = new Dictionary<NevPoint, NevPoint>();
		Dictionary<NevPoint, float> dis = new Dictionary<NevPoint, float>();
		Action<NevPoint, NevPoint, float, float> insertPoint = (point, lastPoint, _dis, priority) => {
			searchQueue.Enqueue(point, priority);
			last.Add(point, lastPoint);
			dis.Add(point, _dis);
		};
		insertPoint(from, from, 0, 0);
		while(searchQueue.Count != 0) {
			NevPoint point = searchQueue.Dequeue();
			if(point == to) break;
			foreach(var dir in Dirs) {
				if(point.GetAroundFlag(dir)) {
					NevPoint tar = GetClosestNevPoint(point.pos + dir * step);
					float nowDis = dis[point] + step;
					float priority = nowDis + (to.pos - tar.pos).magnitude;
					bool contain = last.ContainsKey(tar);
					if(!contain) {
						insertPoint(tar, point, nowDis, priority);
					}
					else if(searchQueue.Contains(tar) && searchQueue.GetPriority(tar) > priority) {
						searchQueue.UpdatePriority(tar, priority);
						last[tar] = point;
						dis[tar] = nowDis;
					}
				}
			}
		}
		Action<Action<NevPoint>> doeachPathNode = (a) => {
			var node = to;
			while(last[node] != from) {
				node = last[node];
				a(node);
			}
		};
		int pathSize = 0;
		doeachPathNode((p)=>pathSize++);
		NevPoint[] result = new NevPoint[pathSize]; //Not contain from and to
		doeachPathNode((p)=>{
			pathSize--;
			result[pathSize] = p;
		});
		return result;
	}
	[Serializable]
	public class NevPoint {
		public Vector2 pos;
		[SerializeField]
		public List<bool> arounds;
		public bool GetAroundFlag(Vector2 dir) {
			return arounds[NevPointMap.GetDirIndex(dir)];
		}
		public void SetAroundFlag(Vector2 dir, bool flag) {
			arounds[NevPointMap.GetDirIndex(dir)] = flag;
		}
		public NevPoint(Vector2 pos) {
			this.pos = pos;
			this.arounds = new List<bool>();
			for(int i = 0; i < NevPointMap.Dirs.Length; i++) {
				this.arounds.Add(false);
			}
		}
	}
	
	[Serializable]
	public class TileMap<T> {
		[HideInInspector]
		[SerializeField]
		List<T> map;
		[SerializeField]
		float tileStep;
		[SerializeField]
		Vector2 originPos;
		[SerializeField]
		int originX;
		[SerializeField]
		int originY;
		[SerializeField]
		int sizeX;
		[SerializeField]
		int sizeY;
		public T this[Vector2 pos] {
			get {
				return GetTile(pos);
			}
			set {
				SetTile(pos, value);
			}
		}
		private T this[int x, int y] {
			get {
				return map[x + y * sizeX];
			}
			set {
				map[x + y * sizeX] = value;
			}
		}
		public TileMap(Rect edge, Vector2 originPos, float tileStep, T initValue) {
			Debug.Assert(edge.Contains(originPos));
			this.originPos = originPos;
			this.tileStep = tileStep;
			Func<Vector2, Vector2> getSize = (dir) => {
				return VectorUtils.Do(dir + new Vector2(1,1) * 0.5f * tileStep, (f) => Mathf.Floor(f / tileStep));
			};
			Vector2 minSize = getSize(originPos - edge.min);
			Vector2 maxSize = getSize(edge.max - originPos);
			Vector2 sumSize = minSize + maxSize + new Vector2(1,1);
			sizeX = (int)sumSize.x;
			sizeY = (int)sumSize.y;
			originX = (int)minSize.x;
			originY = (int)minSize.y;
			map = new List<T>(sizeX * sizeY);
			for(int i = 0; i < sizeX * sizeY; i++) map.Add(initValue);
		}
		private Vector2 GetTileIndex(Vector2 pos)  {
			var dir = pos - originPos + new Vector2(1,1) * 0.5f * tileStep;
			return VectorUtils.Do(dir, (f) => Mathf.Floor(f / tileStep)) + new Vector2(originX, originY);
		}
		private Vector2 GetTileIndexCenter(Vector2 index) {
			Vector2 indexDir = index - new Vector2(originX, originY);
			return originPos + indexDir * tileStep;
		}
		private T GetTile(Vector2 pos) {
			var index = GetTileIndex(pos);
			return GetTileByIndex(index);
		}
		private T GetTileByIndex(Vector2 index) {
			return this[(int)index.x, (int)index.y];
		}
		private void SetTile(Vector2 pos, T t) {
			var index = GetTileIndex(pos);
			this[(int)index.x, (int)index.y] = t;
			return;
		}
		//So slow, bfs
		public T GetClosestTile(Vector2 pos, T nullValue) {
			Vector2 currentIndex = GetTileIndex(pos);
			Vector2[] dirs = NevPointMap.Dirs;
			SimplePriorityQueue<Vector2> searchQueue = new SimplePriorityQueue<Vector2>();
			HashSet<Vector2> searchedSet = new HashSet<Vector2>();
			Action<Vector2> insertIndex = (index) => {
				searchQueue.Enqueue(index, (GetTileIndexCenter(index) - originPos).magnitude);
				searchedSet.Add(index);
			};
			insertIndex(currentIndex);
			while(searchQueue.Count != 0) {
				Vector2 index = searchQueue.Dequeue();
				T tile = GetTileByIndex(index);
				if(!tile.Equals(nullValue)) {
					return tile;
				}
				foreach(var dir in dirs) {
					Vector2 tar = index + dir;
					if(!searchedSet.Contains(tar)) {
						insertIndex(tar);
					}
				}
			}
			return nullValue;
		}
		public void Clear(T value) {
			for(int i = 0; i < sizeX; i++) {
				for(int j = 0; j < sizeY; j++) {
					this[i, j] = value;
				}
			}	
		}
		public bool Contains(Vector2 pos) {
			return this[pos] != null;
		}

    }
    [Serializable]
    public class TileMapInt : TileMap<int>
    {
        public TileMapInt(Rect edge, Vector2 originPos, float tileStep, int initValue) : base(edge, originPos, tileStep, initValue)
        {
        }
    }
	public class Path {
		Vector2[] pointStack;
		int size;
		Vector2 nowPos;
		private Vector2 StackTop {
			get {
				return pointStack[size - 1];
			}
		} 
		private bool StackEmpty {
			get {
				return size == 0;
			}
		}
		public bool Finished{
			get {
				return StackEmpty;
			}
		}
		private void StackPop() {
			size--;
		}
		const float arriveOffset = 0.05f;
		public Path(Vector2 pos, Vector2[] points) {
			var list = new List<Vector2>(points);
			list.Reverse();
			pointStack = list.ToArray();
			size = pointStack.Length;
			nowPos = pos;
		}
		public void Update(Vector2 pos) {
			if(!StackEmpty && (pos - StackTop).magnitude < arriveOffset) {
				StackPop();
			}
			nowPos = pos;
		}
		public Vector2 GetDir() {
			return (StackTop - nowPos);
		}
		public Vector2 GetNormedDir() {
			return GetDir().normalized;
		}
	}
}
