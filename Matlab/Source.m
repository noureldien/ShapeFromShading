clc;
clear all;

light1=[0 0 40];
light2=[20 5 40];
light3=[-5 15 40];
light4=[-10 -10 40];
light5=[5 -10 40];
light6=[5 20 40];

light1 = light1 / norm(light1);
light2 = light2 / norm(light2);
light3 = light3 / norm(light3);
light4 = light4 / norm(light4);
light5 = light5 / norm(light5);
light6 = light6 / norm(light6);

img1 = imread('Images\s_ss1.bmp');
img2 = imread('Images\s_ss2.bmp');
img3 = imread('Images\s_ss3.bmp');
img4 = imread('Images\s_ss4.bmp');
img5 = imread('Images\s_ss5.bmp');
img6 = imread('Images\s_ss6.bmp');
% 
% img1 = imread('Images\s_cc1.bmp');
% img2 = imread('Images\s_cc2.bmp');
% img3 = imread('Images\s_cc3.bmp');
% img4 = imread('Images\s_cc4.bmp');
% img5 = imread('Images\s_cc5.bmp');
% img6 = imread('Images\s_cc6.bmp');
% 
% img1 = imread('Images\ellipse1.bmp');
% img2 = imread('Images\ellipse2.bmp');
% img3 = imread('Images\ellipse3.bmp');
% img4 = imread('Images\ellipse4.bmp');
% img5 = imread('Images\ellipse5.bmp');
% img6 = imread('Images\ellipse6.bmp');

% img1 = imread('Images\image1.jpg');
% img2 = imread('Images\image2.jpg');
% img3 = imread('Images\image3.jpg');
% img4 = imread('Images\image4.jpg');
% img5 = imread('Images\image5.jpg');
% img6 = imread('Images\image6.jpg');

% img1 = imread('Images\image1.jpg');
% img2 = imread('Images\lenna2.jpg');
% img3 = imread('Images\lenna3.jpg');
% img4 = imread('Images\lenna4.jpg');
% img5 = imread('Images\lenna5.jpg');
% img6 = imread('Images\lenna6.jpg');


img1 = imread('Images\camel1.jpg');
img2 = imread('Images\camel2.jpg');
img3 = imread('Images\camel3.jpg');
img4 = imread('Images\camel4.jpg');
img5 = imread('Images\camel5.jpg');
img6 = imread('Images\camel6.jpg');




S= [light1; light2 ;light3; light4; light5; light6];

b=ones(240,320,3);
b=double(b);

p=ones(240, 320);
p=double(p);
q=p;
Z=p;

for i=1:240
    for j=1:320
        
        E=[img1(i,j) img2(i,j) img3(i,j) img4(i,j) img5(i,j) img6(i,j)];
        E=double(E');

        tb= (inv(S'*S))*S'*E;
        
        nbm = norm(tb);
        if( nbm == 0)
            b(i,j,:) = 0; 
        else
            b(i,j,:) = tb / nbm;
        end

        tM = [b(i,j,1) b(i,j,2) b(i,j,3)];
        nbm = norm(tM);
        if( nbm == 0)
            tM = [0 0 0];
        else
            tM = tM / nbm; 
        end        
        p(i,j)=tM(1,1);
        q(i,j)=tM(1,2);
    end
end

for i=1:240
    for j=1:320
        Z(i,j) = sum(q(1:i, 1)) + sum(p(i,1:j));
    end
end
Z = Z*-1;

% figure(1); clf;
% hold on;
% for i=1:2:240
%     for j=1:2:320
%         %i,jø° ¥Î«— ≥Î∏÷ ∫§≈Õ ª—∏≤ 
%         plot3(j+b(i,j,1),i+b(i,j,2),b(i,j,3),'b' );
%     end
% end
% hold off;

figure(2); clf;
mesh(Z);

figure(3); clf;
surf(Z,'EdgeColor','none','FaceColor','red');
camlight headlight;
lighting phong;





