#  Read in a table (in csv format) from standard input:

Table = data.matrix(read.csv( file="stdin", header=TRUE ))
X = Table[, 1:(ncol(Table)-1) ]

# data(iris)
# Table = iris

# X = as.matrix(iris[, 1:4])

classifications = Table[, ncol(Table) ]
k = length(unique(classifications))  #  k is the number of different classifications

y = unclass(classifications)         # convert the class values into numeric indices
# means  = apply(X, 2, mean)
# sigmas = apply(X, 2, sd)
n = nrow(X)
p = ncol(X)

distance_value = matrix(0, nrow=n, ncol=k)  # matrix to record distance values

means  = apply(X, 2, mean)
Sigma = cov(X)  # covariance matrix
detSigma = det(Sigma)

g = function(xvec, meanvec, inverseCovMatrix) {
     1 / sqrt(2*pi)^2 / sqrt(detSigma) *
         exp( -1/2 * ( t(xvec-meanvec) %*% inverseCovMatrix %*% (xvec-meanvec) )[1,1] )
}

# print (X)
for (j in 1:k) { 
    Data_for_j_th_class = subset(X, (y==j) )
    #print (Data_for_j_th_class)
    mean_vector = matrix( apply(Data_for_j_th_class, 2, mean), nrow=p, ncol=1 )  # column vector
    cov_matrix = cov(Data_for_j_th_class)
    SigmaInverse = solve(cov_matrix)    

    #  R's way to compute:  inverse(Sigma)
    # print( as.character(unique(iris$Species))[j] )
    # print( mean_vector )
    # print( cov_matrix )   
    for (i in 1:nrow(X)) {
        distance_value[i,j] = g( c(X[i,]), mean_vector, SigmaInverse )
        # cat(sprintf("i %d  j %d\n", i,j )) 
    }
}
for (i in 1:nrow(X)){
    #print (distance_value[i,])
    jmax = match(max(distance_value[i,]), distance_value[i,])
    #print(distance_value[i,])
    if (jmax != y[i])
        cat(c(i,jmax, y[i],"\n"))
        #cat(sprintf("%d %d %d\n", i,jmin,y[i] )) 
}

# 71 3 2
# 73 3 2
# 84 3 2
# ... For each class j from 1 to k
# ...    Derive the MVN distribution parameters for the j-th class.
# ...    For each row x[i,] in the X matrix,
# ...       distance_value[i,j] = the Gaussian distance_value of x[i,] to class j 
# ...           (using a function like g(), defined above).
# ...
# ... For each row x[i,] in the X matrix,
# ...    If jmin is the number of this closest class and is different from y[i],
# ...    print i, jmin, and y[i].