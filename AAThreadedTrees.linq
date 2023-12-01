<Query Kind="Program" />

void Main()
{
	AATree<int> tree = new AATree<int>();
	//Enumerable.Range(1,10).ToList().ForEach(x => tree.Insert(x));
	tree.Insert(6);
    tree.Insert(2);
    tree.Insert(3);
    //tree.Insert(11);
//    tree.Insert(30);
//    tree.Insert(9);
//    tree.Insert(13);
//    tree.Insert(18);

	//tree.Dump();
	tree.Print().Dump();
	//tree.InOrder().Dump();
	//
	////tree.Delete(9);
	////tree.Delete(7);
	////tree.Delete(18);
	////tree.Delete(30);
	////tree.Delete(11);
	//Enumerable.Range(1,29).ToList().ForEach(x => tree.Delete(x));
	//tree.Print().Dump();
	//tree.InOrder().Dump();
}

//https://en.wikipedia.org/wiki/AA_tree
//http://user.it.uu.se/~arnea/ps/simp.pdf
//https://en.wikipedia.org/wiki/Threaded_binary_tree
//https://www.geeksforgeeks.org/threaded-binary-tree/
public class AANode<T>
where T : IComparable<T>
{
	public int Level { get; set; }
	public AANode<T> Left { get; set; }
	public AANode<T> Right { get; set; }
	public T Data { get; set; }
	 // True if Left points to predecessor in Inorder Traversal 
	public bool IsLeftThreaded { get; set; } // could use 1 bit in level instead of bool
	// True if Right points to successor in Inorder Traversal 
	public bool IsRightThreaded { get; set; } // could use 1 but in level instead of bool
	
	public bool IsLeaf => Left == null && Right == null;
}

public class AATree<T>
where T : IComparable<T>
{
	protected AANode<T> Root { get; set; }

	public void CheckInvariants()
	{
//The level of every leaf node is one.
//The level of every left child is exactly one less than that of its parent.
//The level of every right child is equal to or one less than that of its parent.
//The level of every right grandchild is strictly less than that of its grandparent.
//Every node of level greater than one has two children.
	}
	
	public void Insert(T data)
	{
		//Root = InnerInsert(data, Root);
		InnerInsert(data);
	}
	
	public void Delete(T data)
	{
		Root = InnerDelete(data, Root);
	}
	
	public IEnumerable<T> InOrder()
	{
		List<T> datas = new List<T>();
		InnerInOrder(Root, datas);
		return datas;
	}
	
	private void InnerInOrder(AANode<T> node, List<T> datas)
	{
		if (node.Left != null && !node.IsLeftThreaded)
			InnerInOrder(node.Left, datas);
		datas.Add(node.Data);
		if (node.Right != null && !node.IsRightThreaded)
			InnerInOrder(node.Right, datas);
	}
	
	public string Print()
	{
		if (Root == null)
			return string.Empty;
		StringBuilder sb = new StringBuilder();
		InnerPrint(sb, "", Root, true);
		return sb.ToString();
	}
	
	private void InnerPrint(StringBuilder sb, string indent, AANode<T> node, bool last)
	{
		sb.Append(indent);
		if (last)
		{
			sb.Append("\\-");
			indent += "  ";
		}
		else
		{
			sb.Append("|-");
			indent += "| ";
		}
		sb.AppendLine(node.Data.ToString());

		if (node.Left != null && !node.IsLeftThreaded)
			InnerPrint(sb, indent, node.Left, false);
		if (node.Right != null && !node.IsRightThreaded)
			InnerPrint(sb, indent, node.Right, true);
	}
	
	private void InnerInsert(T data)
	{
		data.Dump("insert:searching");
		AANode<T> parent = null; // parent of the data to be inserted
		AANode<T> node = Root;
		while (node != null)
		{
			node.Data.Dump("insert:cmp");
			int cmp = data.CompareTo(node.Data);
			if (cmp == 0)
				throw new Exception("Duplicate data");
			parent = node; // update parent pointer
			if (cmp < 0) // data <= node.Data
			{
				"insert:going left".Dump();
				if (!node.IsLeftThreaded) 
                	node = node.Left; 
            	else
                	break;
			}
			else //if (cmp > 0) // data > node.Data
			{
				"insert:going left".Dump();
				if (!node.IsRightThreaded) 
                	node = node.Right; 
            	else
                	break;
			}
		}
		
		// Create new node
		"insert:create new node".Dump();
		AANode<T> tmp = new AANode<T>
		{
			Data = data,
			Level = 1,
			Left = null,
			Right = null,
			IsLeftThreaded = false,
			IsRightThreaded = false,
		};
		if (parent == null)
		{
			"insert:no parent".Dump();
			Root = tmp;
			tmp.Left = null;
			tmp.Right = null;
		}
		else
		{
			parent.Data.Dump("insert:non-null parent");
			int cmp = data.CompareTo(parent.Data);
			if (cmp < 0)
			{
				"insert:left insert".Dump();
				tmp.Left = parent.Left;
				tmp.Right = parent;
				tmp.IsRightThreaded = true;
				parent.Left = tmp;
			}
			else
			{
				"insert:right insert".Dump();
				tmp.Left = parent;
				tmp.Right = parent.Right;
				tmp.IsLeftThreaded = true;
				parent.Right = tmp;
			}
		}
		tmp = Skew(tmp);
		tmp = Split(tmp);
		//tmp.Dump("insert-tmp");
		Root = tmp;
	}
	
	private AANode<T> InnerInsert2(T data, AANode<T> node)
	{
		// Normal binary tree insertion procedure
		if (node == null)
			return new AANode<T>
			{
				Data = data,
				Level = 1,
				Left = null,
				Right = null
			};
		else
		{
			int cmp = data.CompareTo(node.Data);
			if (cmp == 0)
				throw new Exception("Duplicate data");
			if (cmp < 0) // data <= node.Data
				node.Left = InnerInsert2(data, node.Left);
			else //if (cmp > 0) // data > node.Data
				node.Right = InnerInsert2(data, node.Right);
		}
		node = Skew(node);
		node = Split(node);
		return node;
	}
	
	private AANode<T> InnerDelete(T data, AANode<T> node)
	{
		if (node == null)
			return null;
		int cmp = data.CompareTo(node.Data);
		if (cmp < 0)
			node.Left = InnerDelete(data, node.Left);
		else if (cmp > 0)
			node.Right = InnerDelete(data, node.Right);
		else // remove this node
		{
			if (node.Left != null)
			{
				AANode<T> heir = Predecessor(node); // find predecessor
				node.Data = heir.Data; // replace value with predecessor
				node.Left = InnerDelete(node.Data, node.Left); // remove predecessor
			}
			else if (node.Right != null)
			{
				AANode<T> heir = Successor(node); // find successor
				node.Data = heir.Data; // replace value with successor
				node.Right = InnerDelete(node.Data, node.Right); // remove successor
			}
			else // leaf
				return null;
		}
		
		// rebalance
		if ((node.Left?.Level ?? 0) < node.Level - 1 || (node.Right?.Level ?? 0) < node.Level - 1)
		{
			node.Level--;
			if ((node.Right?.Level ?? 0) > node.Level)
				node.Right.Level = node.Level;
			node = Skew(node);
			node.Right = Skew(node.Right);
			if (node.Right != null)
				node.Right.Right = Skew(node.Right.Right);
			node = Split(node);
			node.Right = Split(node.Right);
		}
		return node;
	}
	
	// Cannot be used on leaf
	private AANode<T> Predecessor(AANode<T> node)
	{
		node = node.Left;
		while (node.Right != null)
			node = node.Right;
		return node;
	}
	
	// Cannot be used on leaf, called only if left is null
	private AANode<T> Successor(AANode<T> node)
	{
		node = node.Right;
		while (node.Left != null)
			node = node.Left;
		return node;
	}
		
	//        |         |
	//        v         v
	//   L <- T	   ==>  L -> T
	//  /\     \       /     /\
	// A  B     R     A     B  R
	private AANode<T> Skew(AANode<T> node) // node represents T in explanation
	{
		if (node == null)
			return null;
		if (node.Left == null)
			return node;
		if (node.Left.Level == node.Level)
		{
			// swap horizontal links
			AANode<T> tmp = node.Left; // tmp represents L in explanation
			node.Left = tmp.Right;
			tmp.Right = node;
			return tmp;
		}
		return node;
	}
	
	//   |                 |
	//   v                 v
	//   T -> R -> X  ==>  R
	//  /    /             /\
	// A    B             T  X
	//                   /\
	//                  A  B
	private AANode<T> Split(AANode<T> node) // node represents T in explanation
	{
		if (node == null)
			return null;
		if (node.Right == null || node.Right.Right == null)
			return node;
		if (node.Level == node.Right.Right.Level)
		{
			// 2 horizontal links -> take the middle node, elevate it and return it
			AANode<T> tmp = node.Right; // tmp represents R in explanation
			node.Right = tmp.Left;
			tmp.Left = node;
			tmp.Level = tmp.Level + 1;
			return tmp;
		}
		return node;
	}
}