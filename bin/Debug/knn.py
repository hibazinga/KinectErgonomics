class knn:
	def __init__(self, training, labels):
		self.train_data = training
		self.labels = labels
		self.featureNum = len(training[0])

	def test(self,testing,k = 10):
		if len(testing) != self.featureNum:
			print "Number of features error!"
			return -1
		dis_list = []
		
		index = 0
		for ele in self.train_data:
			dis = 0
			for i in range(self.featureNum):
				dis += pow(ele[i] - testing[i],2)
			dis_list.append((pow(dis,0.5),index))
			index += 1
		dis_list.sort()
		#print dis_list

		radiu = dis_list[k-1][0]
		if radiu == 0:
			return self.labels[dis_list[k-1][1]]

		influ = 0

		for i in range(k):
			dis, index = dis_list[i]
			data = self.train_data[index]
			label  = self.labels[index]

			kernel = 0.75 * (1 - pow(dis/radiu,2))

			if label == 1:
				influ += kernel
			else:
				influ -= kernel

		return influ
		if influ > 0:
			return 1
		else:
			return 0


# How to use
# import knn
# classifier_knn = knn.knn([[],[],[]],[,,])
# classifier_knn.test([],k)