<Query Kind="Program" />

void Main()
{
	AATree<int> tree = new AATree<int>();
	//Enumerable.Range(1,10).ToList().ForEach(x => tree.Insert(x));
	tree.Insert(6);
    tree.Insert(2);
    tree.Insert(3);
    tree.Insert(11);
    tree.Insert(30);
    tree.Insert(9);
    tree.Insert(13);
    tree.Insert(18);

	//tree.Dump();
	tree.Print().Dump();
	tree.InOrder().Dump();
	
	//tree.Delete(9);
	//tree.Delete(7);
	//tree.Delete(18);
	//tree.Delete(30);
	//tree.Delete(11);
	Enumerable.Range(1,29).ToList().ForEach(x => tree.Delete(x));
	tree.Print().Dump();
	tree.InOrder().Dump();
}

//https://en.wikipedia.org/wiki/AA_tree
//http://user.it.uu.se/~arnea/ps/simp.pdf
public class AANode<T>
where T : IComparable<T>
{
	public int Level { get; set; }
	public AANode<T> Left { get; set; }
	public AANode<T> Right { get; set; }
	public T Data { get; set; }
}

public class AATree<T>
where T : IComparable<T>
{
	public AANode<T> Root { get; set; }

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
		Root = InnerInsert(data, Root);
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
		if (node.Left != null)
			InnerInOrder(node.Left, datas);
		datas.Add(node.Data);
		if (node.Right != null)
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

		if (node.Left != null)
			InnerPrint(sb, indent, node.Left, false);
		if (node.Right != null)
			InnerPrint(sb, indent, node.Right, true);
	}
	
	private AANode<T> InnerInsert(T data, AANode<T> root)
	{
		// Normal binary tree insertion procedure
		if (root == null)
			return new AANode<T>
			{
				Data = data,
				Level = 1,
				Left = null,
				Right = null
			};
		else if (data.CompareTo(root.Data) <= 0) // data <= root.Data
			root.Left = InnerInsert(data, root.Left);
		else if (data.CompareTo(root.Data) > 0) // data > root.Data
			root.Right = InnerInsert(data, root.Right);
		root = Skew(root);
		root = Split(root);
		return root;
	}
	
	private AANode<T> InnerDelete(T data, AANode<T> root)
	{
		if (root == null)
			return null;
		int cmp = data.CompareTo(root.Data);
		if (cmp < 0)
			root.Left = InnerDelete(data, root.Left);
		else if (cmp > 0)
			root.Right = InnerDelete(data, root.Right);
		else
		{
			//if (root.Left != null && root.Right != null)
			//{
			//	AANode<T> heir = Predecessor(root);
			//	root.Data = heir.Data;
			//	root.Left = InnerDelete(root.Data, root.Left);
			//}
			//else if (root.Left != null)
			//	root = root.Left;
			//else if (root.Right != null)
			//	root = root.Right;
			// is leaf ?
			if (root.Left == null && root.Right == null)
				return root.Right;
			else if (root.Left == null)
			{
				AANode<T> heir = Successor(root);
				root.Data = heir.Data;
				root.Right = InnerDelete(root.Data, root.Right);
			}
			else
			{
				AANode<T> heir = Predecessor(root);
				root.Data = heir.Data;
				root.Left = InnerDelete(root.Data, root.Left);
			}
		}
		
		if ((root.Left?.Level ?? 0) < root.Level - 1 || (root.Right?.Level ?? 0) < root.Level - 1)
		{
			root.Level--;
			if ((root.Right?.Level ?? 0) > root.Level)
				root.Right.Level = root.Level;
			root = Skew(root);
			root.Right = Skew(root.Right);
			if (root.Right != null)
				root.Right.Right = Skew(root.Right.Right);
			root = Split(root);
			root.Right = Split(root.Right);
		}
		return root;
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