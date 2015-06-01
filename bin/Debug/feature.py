import math
 
def vector(node_from, node_to):
    vx = node_to.x - node_from.x
    vy = node_to.y - node_from.y
    vz = node_to.z - node_from.z
    return vx, vy, vz
 
def dist(node_from, node_to):
    vx = node_to.x - node_from.x
    vy = node_to.y - node_from.y
    vz = node_to.z - node_from.z
    return vx * vx + vy * vy + vz * vz
 
# def angle1(v1, v2):    
#   # It's the angle between vector node2->node1 and vector node2->node3
#   cosang = np.dot(v1, v2)
#   sinang = la.norm(np.cross(v1, v2))
#   rad    = np.arctan2(sinang, cosang)
#   deg    = rad / np.pi * 180
#   return deg
 
def angle(v1, v2):    
    # It's the angle between vector node2->node1 and vector node2->node3  
    dot_product = 0
    v1_square_sum = 0
    v2_square_sum = 0
    for i in range(len(v1)):
        dot_product += v1[i] * v2[i]
        v1_square_sum += v1[i] * v1[i]
        v2_square_sum += v2[i] * v2[i]
    cosang = dot_product/math.sqrt(v1_square_sum)/math.sqrt(v2_square_sum)
    rad    = math.acos(cosang)
    deg    = rad / math.pi * 180
    return deg
 
class Features:
    def __init__(self, trial, position):
        self.getVector(trial)
        f1 = self.neck_angle()
        f2 = self.spine_angle()
        f3 = self.hipleft_angle()
        f4 = self.hipright_angle()
        f5 = self.shoulderleft_angle()
        f6 = self.shoulderright_angle()
        f7 = self.elbowleft_angle()
        f8 = self.elbowright_angle()
        f9 = self.wristleft_angle()
        f10 = self.wristright_angle()
        f11 = self.kneeankle_angle()
        f12 = self.kneehip_angle()
        f13 = self.kneeDistance
        f14 = self.kneeleftankles_angle()
 
        if position == 'g': #leg
            self.feature = [f11, f12, f14] 
        elif position == 'm': #arm
            self.feature = [f7, f8] 
        else: #back
            self.feature = [f1, f2, f3, f4] 
        # f11 = self.kneeleft2right_angle()
        # f12 = self.kneeright2left_angle()
         
#       print self.feature
 
    def neck_angle(self):
        return angle(self.shouldercenter2head, self.shouldercenter2spine)
 
    def spine_angle(self):
        return angle(self.shouldercenter2spine, self.spine2hipcenter)
 
    def hipleft_angle(self):
        return angle(self.spine2hipcenter, self.hipcenter2hipleft)
 
    def hipright_angle(self):
        return angle(self.spine2hipcenter, self.hipcenter2hipright)
 
    def shoulderleft_angle(self):
        return angle(self.shouldercenter2shoulderleft, self.shoulderleft2elbowleft)
 
    def shoulderright_angle(self):
        return angle(self.shouldercenter2shoulderright, self.shoulderright2elbowright)
 
    def elbowleft_angle(self):
        return angle(self.shoulderleft2elbowleft, self.elbowleft2wristleft)
 
    def elbowright_angle(self):
        return angle(self.shoulderright2elbowright, self.elbowright2wristright)
 
    def wristleft_angle(self):
        return angle(self.elbowleft2wristleft, self.handleft2wristleft)
 
    def wristright_angle(self):
        return angle(self.elbowright2wristright, self.handright2wristright)
 
    def kneeleft_angle(self):
        return angle(self.hipleft2kneeleft, self.kneeleft2ankleleft)
 
    def kneeright_angle(self):
        return angle(self.hipright2kneeright, self.kneeright2ankleright)
 
    def kneeleftankles_angle(self): #two ankles
        return angle(self.kneeleft2ankleright, self.kneeleft2ankleleft)
 
    def kneerightankles_angle(self): #two ankles
        return angle(self.kneeright2ankleleft, self.kneeright2ankleright)
 
    def kneeankle_angle(self): #left and right separately
        return angle(self.kneeright2ankleright, self.kneeleft2ankleleft)
 
    def kneehip_angle(self): #left and right separately
        return angle(self.hipleft2kneeleft, self.hipright2kneeright)
 
    def getVector(self,sk):
        self.shouldercenter2head         = vector(sk.shoulder_center, sk.head)
        self.shouldercenter2shoulderleft = vector(sk.shoulder_center, sk.shoulder_left)
        self.shouldercenter2shoulderright= vector(sk.shoulder_center, sk.shoulder_right)
        self.shoulderleft2elbowleft      = vector(sk.shoulder_left, sk.elbow_left)
        self.shoulderright2elbowright    = vector(sk.shoulder_right, sk.elbow_right)
        self.elbowleft2wristleft         = vector(sk.elbow_left, sk.wrist_left)
        self.elbowright2wristright       = vector(sk.elbow_right, sk.wrist_right)
        self.handleft2wristleft          = vector(sk.hand_left, sk.wrist_left)
        self.handright2wristright        = vector(sk.hand_right, sk.wrist_right)
        self.shouldercenter2spine        = vector(sk.shoulder_center, sk.spine)
        self.spine2hipcenter             = vector(sk.spine, sk.hip_center)
        self.hipcenter2hipleft           = vector(sk.hip_center, sk.hip_left)
        self.hipcenter2hipright          = vector(sk.hip_center, sk.hip_right)
        self.hipleft2kneeleft            = vector(sk.hip_left, sk.knee_left)
        self.hipright2kneeright          = vector(sk.hip_right, sk.knee_right)
        self.kneeleft2ankleleft          = vector(sk.knee_left, sk.ankle_left)
        self.kneeright2ankleright        = vector(sk.knee_right, sk.ankle_right)
        self.kneeleft2ankleright         = vector(sk.knee_left, sk.ankle_right)
        self.kneeright2ankleleft         = vector(sk.knee_right, sk.ankle_left)
        self.kneeDistance                = dist(sk.knee_left, sk.knee_right)
 
 
# Head:  X: -0.08184159 ,Y: 0.5352136 ,Z: 1.770298
# Shoulder Center:  X: -0.1165376 ,Y: 0.3538453 ,Z: 1.78471
# Shoulder Left:  X: -0.1856502 ,Y: 0.2225188 ,Z: 1.681524
# Shoulder Right:  X: 0.03858259 ,Y: 0.2514651 ,Z: 1.91817
# Spine:  X: -0.05925894 ,Y: 0.04491392 ,Z: 1.803466
# Hip Center:  X: -0.04768175 ,Y: -0.01667549 ,Z: 1.810821
# Hip Left:  X: -0.08511308 ,Y: -0.0864943 ,Z: 1.772421
# Hip Right:  X: 0.01608137 ,Y: -0.0826869 ,Z: 1.856121
# Elbow Left:  X: -0.1733526 ,Y: 0.06311455 ,Z: 1.561112
# Wrist Left:  X: 0.004837948 ,Y: 0.04349827 ,Z: 1.466338
# Hand Left:  X: 0.1362173 ,Y: 0.06716842 ,Z: 1.418823
# Elbow Right:  X: 0.1164613 ,Y: 0.01609685 ,Z: 1.881455
# Wrist Right:  X: 0.2411019 ,Y: 0.0353195 ,Z: 1.736037
# Hand Right:  X: 0.3317491 ,Y: 0.07324887 ,Z: 1.591193
 
# #------------------------------------------------------
 
    # def mean_bp(self, patient, beg=0, end=26):
    #   mean_dia = 0
    #   mean_sys = 0
    #   for datapoint in patient.timeline[beg:end]:
    #       mean_dia += float(datapoint.dia)
    #       mean_sys += float(datapoint.sys)
    #   mean_dia /= 26
    #   mean_sys /= 26
    #   return mean_dia, mean_sys
 
    # def std_bp(self, patient, beg=0, end=26):
    #   std_dia = 0
    #   std_sys = 0
    #   mean_dia, mean_sys = self.mean_bp(patient,beg,end)
    #   for datapoint in patient.timeline[beg:end]:
    #       std_dia += (float(datapoint.dia)-mean_dia)**2
    #       std_sys += (float(datapoint.sys)-mean_sys)**2
    #   std_dia = math.sqrt(std_dia/len(patient.timeline))
    #   std_sys = math.sqrt(std_sys/len(patient.timeline))
    #   return std_dia, std_sys
 
    # def plot_mean_bp(self):
    #   pylab.figure()
    #   x0, x1 = [], []
    #   y0, y1 = [], []
    #   d = self.patient_list
    #   index = 0
 
    #   for patient in self.patient_list:
    #       mean_dia, mean_sys = self.mean_bp(patient)
 
    #       if patient.label == '0':
    #           x0.append(mean_dia)
    #           y0.append(mean_sys)
    #       else:
    #           x1.append(mean_dia)
    #           y1.append(mean_sys)
    #   print x0, y0
    #   pylab.plot(x0, y0, 'ro', x1, y1, 'b^')
 
    #   pylab.axis((50,110,60,180))
    #   pylab.xlabel('diastolic')
    #   pylab.ylabel('systolic')
    #   title = "Mean Patient Blood Pressures"
    #   pylab.title(title)
    #   #pylab.show()
    #   pylab.savefig("./" + title+'.png')
