%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Testing new ShapeFromShade algo
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

clc;

I1=imread('Images\camel2.jpg');
I2=imread('Images\camel3.jpg');
I3=imread('Images\camel4.jpg');
I4=imread('Images\camel5.jpg');

M = size(I1,1);
N = size(I1,2);
images = zeros(M, N, 4);

for i = 1:M
    for j = 1:N
        images(i,j,1) = I1(i,j);
        images(i,j,2) = I2(i,j);
        images(i,j,3) = I3(i,j);
        images(i,j,4) = I4(i,j);
    end
end

% Initializations
albedo=zeros(M,N);
p = zeros(M,N);
q = zeros(M,N);
for i = 1:size(I1,1)
    for j = 1:size(I1,1)
        normal_vector(i,j,1) = 0;
        normal_vector(i,j,2) = 0;
        normal_vector(i,j,3) = 0;
    end
end
%position of light source
L=[1,0,1;
    0,1,1;
    -1,0,1;
    0,-1,1];
normal=[0;0;0];

% processing
for i = 1:M
    for j = 1:N
        I = double([images(i,j,1) images(i,j,2) images(i,j,3) images(i,j,4)]);
        A = L'*L;
        b = L'*I';
        g = inv(A)*b;
        albedo(i,j) = norm(g);
        normal = g/albedo(i,j);
        normal_vector(i,j,1) = normal(1);
        normal_vector(i,j,2) = normal(2);
        normal_vector(i,j,3) = normal(3);
        p(i,j) = normal(1)/normal(3);
        q(i,j) = normal(2)/normal(3);
    end
end
%compute albedo
maxalbedo = max(max(albedo) );
if( maxalbedo > 0)
    albedo = albedo/maxalbedo;
end
%compute depth
depth=zeros(size(I1,1));
for i = 2:size(I1,1)
    for j = 2:size(I1,1)
        depth(i,j) = depth(i-1,j-1)+q(i,j)+p(i,j);
    end
end
%display estimated surface
figure(1);
surfl(depth);
colormap(gray);
grid off;
shading interp