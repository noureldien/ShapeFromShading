%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Shape from shade using variational approach
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

clc;

% read the image
img = imread('images\lenna.png');
img= rgb2gray(img);

lambda = linspace(0.2, 1.4, 6);
figure(1); clf;
for i=1:length(lambda)
    shape = variationalApproach(img, 100, lambda(i));
    subplot(2,3,i);
    imshow(shape);
    title(strcat('Lambda: ', num2str(lambda(i))));
end

lambda = linspace(0.1, 0.6, 6);
figure(2); clf;
for i=1:length(lambda)
    shape = variationalApproach(img, 10, lambda(i));
    subplot(2,3,i);
    imshow(shape);
    title(strcat('Lambda: ', num2str(lambda(i))));
end













