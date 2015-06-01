import parse_data as p
import feature as f

l = p.trial_list.getTrials()
data  = []
label = []
for x in l:
	features = f.Features(x)
	data.append(features.feature)
	if x.head.target == 'good':
		label.append(1)
	else:
		label.append(0)

print len(data)
print len(label)

n = len(data)
import knn

correct = 0
for i in range(n):
	test_data  = data.pop(i)
	test_label = label.pop(i)

	# knn
	cla = knn.knn(data, label)
	if cla.test(test_data,10) == test_label:
		correct += 1
    # nn
    data, label
    test_data, test_label

	data.insert(i, test_data)
	label.insert(i, test_label)
print correct * 1.0 / n