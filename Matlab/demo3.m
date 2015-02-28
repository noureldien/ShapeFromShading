%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Shape from shade using variational approach
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

clc;

% read the image
img = imread('images\lenna.png');
img= rgb2gray(img);
shape = variationalApproach(img, 100, 0.4);
figure(1); clf;
imshow(shape);