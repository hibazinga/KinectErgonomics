import feature as f

n1 = f.Node('12',0,0,1)
n2 = f.Node('12',0,1,0)
n3 = f.Node('12',1,0,0)

fe = f.Feature()
v1 = fe.calvector(n2, n1)
v2 = fe.calvector(n2, n3)
print v1
print v2
print fe.calAngle(v1,v2) 
