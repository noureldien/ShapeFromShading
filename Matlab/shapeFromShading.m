function [Z,k, k_] = shapeFromShading(lights, imageLocation, imageExtension, imageName, imageCount, imageHeight, imageWidth, isGrayScale )
% shape from shading using multible images with their light source

m = imageHeight;
n = imageWidth;
S = lights;

% load images
imgs = zeros(imageCount,m,n);
for i=1:imageCount
    img = imread(strcat(imageLocation, imageName, int2str(i), imageExtension));
    if (isGrayScale)
        imgs(i,:,:) = img;
    else
        imgs(i,:,:) = rgb2gray(img);
    end    
end

% normalize lights
for i=1:size(lights,1)
    light = lights(i,:);
    lights(i,:) = light/norm(light);
end

% for determining the z p, q
b = double(ones(m,n,3));
doubleOnes = double(ones(m,n));
p = doubleOnes;
q = doubleOnes;
% the actual depth of the entrance to the Z
Z = doubleOnes;
k = doubleOnes;

for i=1:m
    for j=1:n
        E=imgs(:,i,j);
        tb= (inv(S'*S))*S'*E;
        
        % tb is the final exit 3x1.
        nbm = norm(tb);
        
        if( nbm == 0)
            b(i,j,:) = 0;
        else
            % the tb brightness -> divided by norm (tb) is the normal vector becomes old
            b(i,j,:) = tb / nbm;
        end
        
        % % looking for a calculated p q for the Z
        tM = [b(i,j,1) b(i,j,2) b(i,j,3)];
        nbm = norm(tM);
        if( nbm == 0)
            tM = [0 0 0];
        else
            tM = tM / nbm;
        end
        
        p(i,j)=tM(1,1);
        q(i,j)=tM(1,2);
        k(i,j)=tM(1,3);
    end
end

% and store the Z value using the pq
for i=1:m
    for j=1:n
        Z(i,j) = sum(q(1:i, 1)) + sum(p(i,1:j));
    end
end
Z = Z*-1;

gaussian = fspecial('gaussian',[5 5],3);
k_ = imfilter(k,gaussian,'same');

end




